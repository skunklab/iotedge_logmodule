using LogModule;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = "/share";
            //string filename = "deploy.txt";
            //string blobPath = "hackfestcontainer";
            //string blobFilename = "deploy.txt";

            //LogModule.Models.DownloadFileModel model = new LogModule.Models.DownloadFileModel(path, filename, blobPath, blobFilename, false);
            //string jsonString = JsonConvert.SerializeObject(model);
            //Console.ReadKey();

            /*
             * STORAGE_ACCOUNT_NAME=acmblobstorage
                STORAGE_ACCOUNT_KEY=Bz881iXO88F2ryI3Q6YVWiEU28TGrrk6gcbV0E99H/UX0tQAanpjpNCJzpO/F2Ms36l0hkIMFoc6MkfrY07S8Q==

                Filename: package.zip
                Container: hackfestcontainer
             */

            string accountName = "acmblobstorage";
            string accountKey = "Bz881iXO88F2ryI3Q6YVWiEU28TGrrk6gcbV0E99H/UX0tQAanpjpNCJzpO/F2Ms36l0hkIMFoc6MkfrY07S8Q==";
            ContainerRemote remote = ContainerRemote.Create(accountName, accountKey);
            remote.OnDownloadBytesTransferred += Remote_OnDownloadBytesTransferred;
            remote.OnDownloadCompleted += Remote_OnDownloadCompleted;
            remote.OnUploadBytesTransferred += Remote_OnUploadBytesTransferred;
            remote.OnUploadCompleted += Remote_OnUploadCompleted;

            Console.WriteLine("Press to start");
            Console.ReadKey();
            CancellationTokenSource cts = new CancellationTokenSource();
            Task tt = remote.UploadFile("c:\\", "large_package.zip", "hackfestcontainer", "large_package_upload_test2.zip", "application/zip", false, cts.Token);
            Task.WhenAll(tt);

            //Task task = remote.DownloadFile("c:\\", "large_package.zip", "hackfestcontainer", "large_package.zip", false, cts.Token);
            //Task.WaitAll(task);

            Task dtask = Task.Delay(1000);
            Task.WaitAll(dtask);

            cts.Cancel();

            Console.WriteLine("Done");
            Console.ReadKey();




        }

        private static void Remote_OnUploadCompleted(object sender, BlobCompleteEventArgs e)
        {
            Console.WriteLine("Upload {0}", e.Cancelled ? "Cancelled" : "Completed");
        }

        private static void Remote_OnUploadBytesTransferred(object sender, BytesTransferredEventArgs e)
        {
            Console.WriteLine("{0} : {1} : {2}", e.Filename, e.BytesTransferred, e.Length);
        }

        private static void Remote_OnDownloadCompleted(object sender, BlobCompleteEventArgs e)
        {
            Console.WriteLine("Download {0}", e.Cancelled ? "Cancelled" : "Completed");
        }

        private static void Remote_OnDownloadBytesTransferred(object sender, BytesTransferredEventArgs e)
        {
            Console.WriteLine("{0} : {1} : {2}", e.Filename, e.BytesTransferred, e.Length);
        }
    }
}
