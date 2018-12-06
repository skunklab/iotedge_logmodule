# IoT Edge Log Module

## REST API Client Samples

### Links
   - [ReadMe](../readme.md)
   - [Direct Methods API](dmi.md)
   - [REST API](logmodulerestapi.md)

# Introduction

The IoTEdge.LogModule.Core Nuget package allows a client to quickly use the Log Module REST API. The following samples demonstrate how to call the APIs.

For all samples, the log module is deployed in a docker container to an folder named &quot;app&quot;.  The docker volume &quot;data&quot; is bound as a subfolder of the &quot;app&quot; folder, i.e., app/data.  Therefore, a path to the docker volume &quot;data&quot; relative to the log module is &quot;./data&quot;.

### _Notes on Appending Files_

Several methods allow you to append data to an existing file.  When used, the append with prepend &quot;\r\n&quot; to the data written to create a new line of data.  The common scenario is logging the same formatted text, e.g., telemetry, ML statistics, alerts, etc., which is generally represented as lines of data collected in a file.

### _Notes on Blob Storage paths and filenames_

We use 2 parameters for identification of files within Azure Blob storage, i.e., blobPath and blobFilename.  The blobFilename is simply the name of the file, e.g., &quot;myfile.txt&quot;.  The blobPath is used like a folder path, where the blob container is the top level folder.  For example, blobPath=&quot;data&quot; will write a file to the &quot;data&quot; container in Azure Blob storage.  You can use subfolders in the blobPath by appending the subfolders to the blobPath, e.g., blobPath=&quot;data/texas/telemetry&quot;, would write a file to the container &quot;data&quot; with subfolders of &quot;texas/telemetry&quot;.

## Get File Sample

Returns a file as an array of bytes from a docker volume relative to the log module.
```
public static async Task<byte[]> GetFileAsync()
{
    //gets a file from the docker volume

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    return await client.GetFile(path, filename);
}
```
## Delete File Sample

Removes a file from a docker volume relative to the log module.
```
public static async Task RemoveFileAsync()
{
    //removes a file from the docker volume

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    await client.RemoveFile(path, filename);
}
```
## Write New File Sample

Writes a new file to the docker volume relative to the log module.
```
public static async Task WriteNewFileAsync()
{
    //writes a new file to a docker volume

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    byte[] body = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog.");
    bool append = false;
    await client.WriteFile(path, filename, body, append);
}
```
## Append to a File Sample

Appends to an existing file in a docker volume relative to the log module.
```
public static async Task AppendFileAsync()
{
    //appends to an existing file in a docker volume

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    byte[] body = Encoding.UTF8.GetBytes("Add another line of text :-)");
    bool append = true;
    await client.WriteFile(path, filename, body, append);
}
```


## Get File List Sample

Return a string array of files in a docker volume relative to the log module.
```
public static async Task<string[]> GetFileListAsync()
{
    //gets a list of files in a docker volume

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    returnawait client.ListFiles(path);
}
```
## Compress File Sample

Compress a file located in a docker volume relative to the log modules and writes the compressed file to a docker volume relative to the log module.
```
public static async Task CompressFileAsnyc()
{
    //compresses and existing file in a docker volume
    //and write the compressed file to a docker volume

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    string compressPath = "./data";
    string compressFilename = "test.zip";
    await client.CompressFile(path, filename, compressPath, compressFilename);
}
```
## Truncate File Sample

Truncates a file in a docker volume relative to the log module.
```
public static async Task TruncateFileAsync()
{
    //truncates a file in a docker volume to
    //maximum number of bytes

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    int maxBytes = 40;
    await client.TruncateFile(path, filename, maxBytes);
}
```
# Upload File Sample

Uploads a file in a docker volume relative to the log module to Azure Blob storage.
```
public static async Task UploadFileAsync()
{
    //upload file from docker volume and write file to blob storage

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    string blobPath = "mycontainer";
    string blobFilename = "mytest.txt";
    string contentType = "text/plain";
    bool append = false;
   await client.UploadFile(path, filename, blobPath, blobFilename, contentType, append);
}
```
## Upload File using SAS URI Sample

Uploads a file in a docker volume relative to the log module to Azure Blob storage using a SAS URI to the blob with writable permissions.
```
public static async Task UploadFileBySasUriAsync()
{
    //upload file from docker volume and write file
    //to blob storage using a SAS URI

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    string sasUri = "https://<long-sas-uri>";
    string contentType = "text/plain";
    bool append = false;
    await client.UploadFile(path, filename, sasUri, contentType, append);
}
```
## Upload a File and Append to a Blob Sample

Uploads a file in a docker volume relative to the log module to Azure Blob storage and appends to a file in Azure Blob storage.
```
public static async Task UploadFileAndAppendAsync()
{
    //upload file from docker volume and
    //write file as an append file to blob storage

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    string blobPath = "mycontainer";
    string blobFilename = "mytest.txt";
    string contentType = "text/plain";
    bool append = true;
    await client.UploadFile(path, filename, blobPath, blobFilename, contentType, append);
}
```
## Upload a File using a SAS URI and Append to a Blob Sample

Uploads a file in a docker volume relative to the log module and appends to a file in Azure Blob storage using a SAS URI to the blob with writable permissions.
```
public static async Task UploadFileBySasUriAndAppendAsync()
{
    //upload file from docker volume and write file as
    //an append file to blob storage using a SAS URI

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    string sasUri = " https://<long-sas-uri>";
    string contentType = "text/plain";
    bool append = true;
    await client.UploadFile(path, filename, sasUri, contentType, append);
}
```
## Download a File Sample

Downloads a file from Azure Blob storage and writes the file to a docker volume relative to the log module.
```
public static async Task DownloadFileAsync()
{
    //downloads a file from blob storage and
    //writes to docker volume

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    string blobPath = "mycontainer";
    string blobFilename = "mytest.txt";
    bool append = false;
    await client.DownloadFile(path, filename, blobPath, blobFilename, append);
}
```
## Download a Blob Using a SAS URI Sample

Downloads a file from Azure Blob storage using SAS URI with read permissions and writes the file to a docker volume relative to the log module.

public static async Task DownloadFileBySasUriAsync()
{
    //downloads a file from blob storage via SAS URI
    //and write to docker volume

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    string sasUri = " https://<long-sas-uri>";
    bool append = false;
    await client.DownloadFile(path, filename, sasUri, append);
}
```
## Download a Blob and Append to a File Sample

Downloads a file from Azure Blob storage and appends to a file in a docker volume relative to the log module.
```
public static async Task DownloadFileAndAppendAsync()
{
    //downloads a file from blob storage and appends to
    //an existing file in docker volume

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    string blobPath = "mycontainer";
    string blobFilename = "mytest.txt";
    bool append = true;
    await client.DownloadFile(path, filename, blobPath, blobFilename, append);
}
```
## Download a Blob Using a SAS URI and Append to a File Sample

Downloads a file from Azure Blob storage using SAS URI with read permissions and appends to a file in a docker volume relative to the log module.
```
public static async Task DownloadFileBySasUriAndAppendAsync()
{
    //downloads a file from blob storage and appends to
    //an existing file in docker volume

    string containerName = "iotedge-logmodule";
    int port = 8877;
    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);
    string path = "./data";
    string filename = "test.txt";
    string sasUri = " https://<long-sas-uri>";
    bool append = true;
    await client.DownloadFile(path, filename, sasUri, append);
}
```					
