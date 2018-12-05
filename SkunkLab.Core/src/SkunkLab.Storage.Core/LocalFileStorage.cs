using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Storage
{
    public class LocalFileStorage
    {
        public LocalFileStorage()
        {
            container = new HashSet<string>();
        }

        private HashSet<string> container;
        public async Task<byte[]> ReadFileAsync(string path, CancellationToken token)
        {
            if(string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if(!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            byte[] message = null;

            try
            {
                if (container.Contains(path.ToLowerInvariant()))
                {
                    await AccessWaitAsync(path, TimeSpan.FromSeconds(20.0));
                }

                container.Add(path.ToLowerInvariant());

                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {

                    int bytesRead = 0;
                    do
                    {
                        if (token.IsCancellationRequested)
                        {
                            message = null;
                            break;
                        }

                        byte[] buffer = new byte[ushort.MaxValue];
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                        if (message == null)
                        {
                            message = new byte[bytesRead];
                            Buffer.BlockCopy(buffer, 0, message, 0, bytesRead);
                        }
                        else
                        {
                            byte[] temp = new byte[message.Length + buffer.Length];
                            Buffer.BlockCopy(message, 0, temp, 0, message.Length);
                            Buffer.BlockCopy(buffer, 0, temp, message.Length, buffer.Length);
                            message = temp;
                        }
                    } while (bytesRead > 0);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TaskCanceledException)
                {
                    message = null;
                }
                else
                {
                    throw ex;
                }
            }
            finally
            {
                container.Remove(path.ToLowerInvariant());
            }

            return message;
        }

        public async Task WriteFileAsync(string path, byte[] source, CancellationToken token)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
            
            try
            {
                if (container.Contains(path.ToLowerInvariant()))
                {
                    await AccessWaitAsync(path, TimeSpan.FromSeconds(20.0));
                }

                container.Add(path.ToLowerInvariant());

                using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    int offset = 0;
                    do
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        int length = offset + ushort.MaxValue >= source.Length ? source.Length - offset : offset + ushort.MaxValue;
                        await stream.WriteAsync(source, offset, length);
                        offset += length;
                    } while (offset + ushort.MaxValue < source.Length);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TaskCanceledException)
                {
                }
                else
                {
                    throw ex;
                }
            }
            finally
            {
                container.Remove(path.ToLowerInvariant());
            }
        }
        
        public async Task AppendFileAsync(string path, byte[] source, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            byte[] existing = await ReadFileAsync(path, token);

            string crlf = "\r\n";
            byte[] crlfBytes = Encoding.UTF8.GetBytes(crlf);
            byte[] buffer = new byte[crlfBytes.Length + source.Length + existing.Length];
            Buffer.BlockCopy(existing, 0, buffer, 0, existing.Length);
            Buffer.BlockCopy(crlfBytes, 0, buffer, existing.Length, crlfBytes.Length);
            Buffer.BlockCopy(source, 0, buffer, existing.Length + crlfBytes.Length, source.Length);

            await WriteFileAsync(path, buffer, token);
        }

        public async Task TruncateFileAsync(string path, int maxBytes, CancellationToken token = default(CancellationToken))
        {
            if(string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if(maxBytes < 0)
            {
                throw new IndexOutOfRangeException("maxBytes");
            }

            if(!File.Exists(path))
            {
                throw new FileNotFoundException("path");
            }

            byte[] source = await ReadFileAsync(path, token);

            if(source.Length <= maxBytes)
            {
                return;
            }

            byte[] buffer = new byte[maxBytes];
            Buffer.BlockCopy(source, 0, buffer, 0, buffer.Length);
            await WriteFileAsync(path, buffer, token);
        }


        private bool CanAccess(string filename)
        {
            return !container.Contains(filename.ToLowerInvariant());
        }
        private async Task AccessWaitAsync(string filename, TimeSpan maxWaitTime)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (!CanAccess(filename))
            {
                await Task.Delay(100);
                if (stopwatch.ElapsedMilliseconds > maxWaitTime.TotalMilliseconds)
                {
                    break;
                }
            }

            stopwatch.Stop();
        }
    }
}
