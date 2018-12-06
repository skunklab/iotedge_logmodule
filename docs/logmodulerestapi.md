# IoT Edge Log Module
## REST API

### Links
   - [ReadMe](../README.md)
   - [DotNet Client Samples](./foo.md)
   - [Direct Methods API](./dmi.md)

# Introduction

The following are the descriptions of the REST API for the log module, which can be used to build your clients to operations.  The IoTEdge.LogModule.Core nuget package contains a client that can be used to call this API if that is an option.

# Get File

Returns a file as an array of bytes for a docker volume.  If the file does not exist null will be returned.  The path parameter is relative to the &quot;Bind&quot; of the docker volume to the Log Module.  The Log Module is deployed in the /app folder.

**Request URL** : http://&lt;ip&gt;:&lt;port&gt;/api/Log/GetFile

**Method** : GET

**Content-Type** : NM

**Accept** : application/octet-stream

**Body** : NM

**Response** : 200 OK (success), 500 Internal Server Error (fault)

**Query String Parameters**

| **Name** | **Description** |
| --- | --- |
| Path | Folder path of docker volume relative to the log module, e.g., &quot;./data&quot; |
| Filename | The name of the file to get. |



# Write File

Writes file to a docker volume.  If append is TRUE, the write will append to an existing file by prepending \r\n and the data sent. If append is TRUE and the file does not exist, the file will be created.  If append is FALSE, then a new file will be created, even if one previously exists.

**Request URL** : http://&lt;ip&gt;:&lt;port&gt;/api/Log/WriteFile

**Method** : POST

**Content-Type** : application/octet-stream

**Accept** : NM

**Body** : Array of bytes

**Response** : 200 OK (success), 500 Internal Server Error (fault)

**Query String Parameters**

| **Name** | **Description** |
| --- | --- |
| Path | Folder path of docker volume relative to the log module, e.g., &quot;./data&quot; |
| Filename | The name of the file to write. |
| Append | TRUE is the data is to be appended to an existing file; otherwise FALSE |

# Remove File

Removes a file from a docker volume.

**Request URL** : http://&lt;ip&gt;:&lt;port&gt;/api/Log/RemoveFile

**Method** : DELETE

**Content-Type** : NM

**Accept** : NM

**Body** : NM

**Response** : 200 OK (success), 500 Internal Server Error (fault)

**Query String Parameters**

| **Name** | **Description** |
| --- | --- |
| Path | Folder path of docker volume relative to the log module, e.g., &quot;./data&quot; |
| Filename | The name of the file to delete. |

# Truncate File

Truncate File is an operation that removes data from the beginning of a file when the file exceeds the MaxBytes parameter.

**Request URL** : http://&lt;ip&gt;:&lt;port&gt;/api/Log/TruncateFile

**Method** : PUT

**Content-Type** : NM

**Accept** : NM

**Body** : NM

**Response** : 200 OK (success), 500 Internal Server Error (fault)

**Query String Parameters**

| **Name** | **Description** |
| --- | --- |
| Path | Folder path of docker volume relative to the log module, e.g., &quot;./data&quot; |
| Filename | The name of the file to truncate. |
| MaxBytes | Maximum number of bytes the file may have. |

# List Files

Reads a docker volume and returns a list of the files in the volume as a JSON string array.

**Request URL** : http://&lt;ip&gt;:&lt;port&gt;/api/Log/ListFiles

**Method** : GET

**Content-Type** : NM

**Accept** : application/json

**Body** : NM

**Response** : 200 OK (success) , 500 Internal Server Error (fault)

**Query String Parameters**

| **Name** | **Description** |
| --- | --- |
| Path | Folder path of docker volume relative to the log module, e.g., &quot;./data&quot; |



# Compress File

Compress and existing file and writes as a new file, i.e., compression mode is zip.

**Request URL** : http://&lt;ip&gt;:&lt;port&gt;/api/Log/CompressFile

**Method** : POST

**Content-Type** : NM

**Accept** : NM

**Body** : NM

**Response** : 200 OK (success), 500 Internal Server Error (fault)

**Query String Parameters**

| **Name** | **Description** |
| --- | --- |
| Path | Folder path of docker volume relative to the log module, e.g., &quot;./data&quot; of the file to compress |
| Filename | File name of file to compress |
| CompressPath | Folder path of docker volume relative to the log module, e.g., &quot;./data&quot; where the compressed file will be written |
| CompressFilename | File name of the compresed file, i.e., myfile.zip. |



# Download File

Download a file from Azure Blob storage and stores it locally.  The Download File operations are 2 signatures, i.e., one for download directly by container name and file name, the other using SAS URI to locate the file in Azure Blob storage.

**Request URL** : http://&lt;ip&gt;:&lt;port&gt;/api/Log/DownloadFile

**Method** : GET

**Content-Type** : NM

**Accept** : NM

**Body** : NM

**Response** : 200 OK (success), 500 Internal Server Error (fault)

**Query String Parameters**

| **Name** | **Description** |
| --- | --- |
| path | Folder path of docker volume relative to the log module, e.g., &quot;./data&quot; |
| filename | The name of the file written after download. |
| blobPath | The path to the blob file starting with the container e.g., if a subfolder exists the blob path would be &lt;container&gt;/subfolder |
| blobFilename | The name of the blob file |
| append | If TRUE the downloaded contents will be appended to an existing file; otherwise the download will be a new file or replace an existing file. |



**Request URL** : http://&lt;ip&gt;:&lt;port&gt;/api/Log/DownloadFile2

**Method** : GET

**Content-Type** : NM

**Accept** : NM

**Body** : NM

**Response** : 200 OK (success), 500 Internal Server Error (fault)

**Query String Parameters**

| **Name** | **Description** |
| --- | --- |
| path | Folder path of docker volume relative to the log module, e.g., &quot;./data&quot; |
| filename | The name of the file written after download. |
| sasURI | SAS URI of the blob to be downloaded. |
| append | If TRUE the downloaded contents will be appended to an existing file; otherwise the download will be a new file or replace and existing file. |

# Upload File

Upload a local file to Azure Blob storage.  The Upload File operations are 2 signatures, i.e., one for uploading directly via container name and file name, the other using SAS URI to upload the file in Azure Blob storage.

**Request URL** : http://&lt;ip&gt;:&lt;port&gt;/api/Log/UploadFile

**Method** : POST

**Content-Type** : NM

**Accept** : NM

**Body** : NM

**Response** : 200 OK (success), 500 Internal Server Error (fault)

**Query String Parameters**

| **Name** | **Description** |
| --- | --- |
| path | Folder path of docker volume relative to the log module, e.g., &quot;./data&quot; |
| filename | The name of the file to upload. |
| blobPath | The path to the blob file starting with the container e.g., if a subfolder exists the blob path would be &lt;container&gt;/subfolder |
| blobFilename | The name of the blob file |
| contentType | The content type of the file to set the property in Azure Blob storage, e.g., text/plain, etc. |
| append | If TRUE the uploaded contents will be appended to an existing file; otherwise the upload will be a new file or replace an existing file. |



**Request URL** : http://&lt;ip&gt;:&lt;port&gt;/api/Log/UploadFile2

**Method** : POST

**Content-Type** : NM

**Accept** : NM

**Body** : NM

**Response** : 200 OK (success), 500 Internal Server Error (fault)

**Query String Parameters**

| **Name** | **Description** |
| --- | --- |
| path | Folder path of docker volume relative to the log module, e.g., &quot;./data&quot; |
| filename | The name of the file to upload. |
| sasUri | SAS URI of the blob to be uploaded |
| contentType | The content type of the file to set the property in Azure Blob storage, e.g., text/plain, etc. |
| append | If TRUE the uploaded contents will be appended to an existing file; otherwise the upload will be a new file or replace an existing file. |
