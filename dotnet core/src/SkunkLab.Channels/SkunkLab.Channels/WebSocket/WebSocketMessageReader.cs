

namespace SkunkLab.Channels.WebSocket
{
   
        using System;
        using System.Net.WebSockets;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;

        internal static class WebSocketMessageReader
        {
            private static byte[] BufferSliceToByteArray(byte[] buffer, int count)
            {
                byte[] dst = new byte[count];
                System.Buffer.BlockCopy(buffer, 0, dst, 0, count);
                return dst;
            }

            private static string BufferSliceToString(byte[] buffer, int count) =>
                Encoding.UTF8.GetString(buffer, 0, count);

            public static async Task<WebSocketMessage> ReadMessageAsync(WebSocket webSocket, byte[] buffer, int maxMessageSize, CancellationToken token)
            {
                WebSocketMessage message;
                ArraySegment<byte> arraySegment = new ArraySegment<byte>(buffer);
                WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(arraySegment, token).WithCancellation<WebSocketReceiveResult>(token);
                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    message = new WebSocketMessage(null, WebSocketMessageType.Close);
                }
                else
                {
                    if (receiveResult.EndOfMessage)
                    {
                        switch (receiveResult.MessageType)
                        {
                            case WebSocketMessageType.Text:
                                return new WebSocketMessage(BufferSliceToString(buffer, receiveResult.Count), WebSocketMessageType.Text);

                            case WebSocketMessageType.Binary:
                                return new WebSocketMessage(BufferSliceToByteArray(buffer, receiveResult.Count), WebSocketMessageType.Binary);
                        }
                        throw new Exception("This code path should never be hit.");
                    }
                    ByteBuffer bytebuffer = new ByteBuffer(maxMessageSize);
                    bytebuffer.Append(BufferSliceToByteArray(buffer, receiveResult.Count));
                    WebSocketMessageType messageType = receiveResult.MessageType;
                    while (true)
                    {
                        receiveResult = await webSocket.ReceiveAsync(arraySegment, token);
                        if (receiveResult.MessageType != messageType)
                        {
                            throw new InvalidOperationException("Wrong message type.");
                        }
                        bytebuffer.Append(BufferSliceToByteArray(buffer, receiveResult.Count));
                        if (receiveResult.EndOfMessage)
                        {
                            switch (receiveResult.MessageType)
                            {
                                case WebSocketMessageType.Text:
                                    return new WebSocketMessage(bytebuffer.GetString(), WebSocketMessageType.Text);

                                case WebSocketMessageType.Binary:
                                    return new WebSocketMessage(bytebuffer.GetByteArray(), WebSocketMessageType.Binary);
                            }
                            throw new Exception("This code path should never be hit.");
                        }
                    }
                }
                return message;
            }
        }
    
}
