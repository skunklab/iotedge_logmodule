# IoT Edge Log Module
## Direct Methods API

### Links
   - [ReadMe](../README.md)
   - [DotNet Client Samples](dotnetrestclientsamples.md)
   - [REST API](logmodulerestapi.md)

# Introduction
The Direct Methods interface (DMI) is intended for remote management of the Log Module.  The DMI allows a user to perform file upload, download, compression, removal, and truncation of local files on the IoT Edge device.  

## Upload File
Uploads a file from an IoT Edge device to Azure Blob storage.  

**Direct Method Name:** uploadFile


| **Parameter** | **Description**                                                                                                    |
|---------------|--------------------------------------------------------------------------------------------------------------------|
| path          | Folder of the file to upload                                                                                       |
| filename      | Name of file to upload                                                                                             |
| contentType   | Content type of file, e.g., application/json                                                                       |
| blobPath      | Container name to upload file, syntax /container/subfolder/. Required when “sasUri” is omitted.                    |
| blobFilename  | Name of file after uploaded to blob storage. Required when “sasUri” is omitted.                                    |
| sasUri        | Shared Access Token (SAS) URI that is writable to upload file. Omitted when “blobPath” and “blobFilename” is used. |
| deleteOnUpload| If TRUE the local file will be deleted after upload is completed; otherwise the loal file will remain.               |
| ttl| A Time-To-Live (TimeSpan) for a local file to be upload. If the TTL is exceeded, the local file will be deleted. If an exception occurs the processor will continue attempts to upload the file until the TTL expires, i.e. one every 60 seconds.              |
| append        | Appends to an existing blob when TRUE using \\r\\n. Otherwise creates a new file. Default is FALSE.                |
| cancel        | Cancels an in-progress upload to blob storage when TRUE. Otherwise will attempt file upload. Default is FALSE.     |


### Sample JSON 
Upload a file local to the edge device in the docker volume ./data and filename "test.txt" to Azure Blob storage in the container "testcontainer" with the filename of "mytestfile.txt".

```
{

"path": "./data",
"filename": "test.txt",
"contentType": "text/plain",
"blobPath": "/testcontainer",
"blobFilename": "mytestfile.txt",
"sasUri": null,
 "deleteOnUpload": true,
  "ttl":"01:00:00",
"append": false,
"cancel": false
}
```


## Download File
Uploads a file from Azure Blob storage to a docker volume on an IoT Edge device.

**Direct Method Name:** downloadFile


| **Parameter** | **Description**                                                                                                    |
|---------------|--------------------------------------------------------------------------------------------------------------------|
| path          | Folder of where file will be download.                                                                                       |
| filename      | Name of file downloaded.                                                                                             |
| blobPath      | Container name of file to download, syntax /container/subfolder/. Required when “sasUri” is omitted.                    |
| blobFilename  | Name of file to be downloaded from blob storage. Required when “sasUri” is omitted.                                    |
| sasUri        | Shared Access Token (SAS) URI that is readable to download file. Omitted when “blobPath” and “blobFilename” are used. |
| append        | Appends to an existing local file when TRUE using \\r\\n. Otherwise creates a new file. Default is FALSE.                |
| cancel        | Cancels an in-progress download from blob storage when TRUE. Otherwise will attempt file download. Default is FALSE.     |


### Sample JSON 
Downloads a file from Azure Blob storage in the container "testcontainer" with the filename "mytestfile.txt".  The file is download to the edge device in the docker volume ./data with filename "test.txt".

```
{
  "path": "./data",
  "filename": "test.txt",
  "blobPath": "/testcontainer",
  "blobFilename": "mytestfile.txt",
  "sasUri": null,
  "append": false,
  "cancel": false
}
```

## Remove File
Deletes a file from an IoT Edge device.

**Direct Method Name:** removeFile

| **Parameter** | **Description**                                                                                                    |
|---------------|--------------------------------------------------------------------------------------------------------------------|
| path          | Folder of where file will be download.                                                                                       |
| filename      | Name of file downloaded.                                                                                           |
### Sample JSON 
Deletes a file in the docker volume "./data" with the filename "test.txt" from an IoT Edge device.
```
{
  "path": "./data",
  "filename": "test.txt"
}
```

## Compress File
Compresses a file as zip file and writes it to a docker volume on an IoT Edge device.

**Direct Method Name:** compressFile

| **Parameter** | **Description**                                                                                                    |
|---------------|--------------------------------------------------------------------------------------------------------------------|
| path          | Folder path where file to be compressed is located.                                                                                       |
| filename      | Name of file to compress.                                                                                             |
| compressPath      | Folder path where compressed file will be written.                    |
| compressFilename  | Name of compressed file to be written.                                    |
### Sample JSON 
Compresses a file "test.txt" in the docker volume "./data" and writes the compressed file "compressedtext.zip" to the same docker volume, i.e., "./data".

```
{
  "path": "./data",
  "filename": "test.txt",
  "compressPath": "./data",
  "compressFilename": "compressedtext.zip"
}
```

## Truncate File
Truncates a file in a docker volume on an IoT Edge device to a defined size in bytes. If the file is smaller than the truncation size, the file will not be altered.

**Direct Method Name:** truncateFile

| **Parameter** | **Description**                                                                                                    |
|---------------|--------------------------------------------------------------------------------------------------------------------|
| path          | Folder path where file to be compressed is located.                                                                                       |
| filename      | Name of file to compress.                                                                                             |
| maxBytes      | Maximum size of the file in bytes.                    |

### Sample JSON 
Truncates a file "test.txt" in the docker volume "./data" to a size of 100K bytes. If the file is smaller than 100K, it will not be altered.
```
{
  "path": "./data",
  "filename": "test.txt",
  "maxBytes": 100000
}
```

