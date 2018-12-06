# IoT Edge Log Module

# Introduction

The core scenario is driven by the need to store information locally on an edge device and administer that information either locally or remotely.  There are numerous scenarios in play including offline, storing data locally for retraining of ML models, and acquisition of configuration information unavailable for certain scenarios.  These drive a fundamental scenario where persistent data is required and needs to be managed on edge devices.

The Log Module is publicly available with

- [Source](https://github.com/skunklab/iotedge_logmodule)
- [Nuget Package](https://www.nuget.org/packages/IoTEdge.LogModule.Core/0.9.8-prerelease) for creating clients
- [Docker Container](https://hub.docker.com/r/skunklab/iotedge-logmodule/) ready-to-use

# IoT Edge Log Module APIs

The Log module provides 3 APIs which 1 or more can be configure for use.

- REST API for inter-module operations
- Edge Hub API for inter-module operations
- Direct Methods API for either inter-module operations or external administration.

# Features

| **Feature** | **Description** | **Interfaces** |
| --- | --- | --- |
| Get File | Returns a file as byte array from local storage | 1,2 |
| Write File | Writes a file to local storage with an optional parameter to write as an append file prepending \r\n to the write operation. | 1,2 |
| List Files | Return a string[] of file names in a folder. |   |
| Remove File | Deletes a file | 1,2 |
| Truncate File | Remove the first &quot;x&quot; bytes from the beginning of a file. | 1,2,3 |
| Compress File | Creates a new zip file from an existing file | 1,2,3 |
| Upload File | Upload a file from local storage to Azure Blob storage with an optional parameter to write to the blob as an append file. | 1,2,3 |
| Download File | Downloads a file from Azure Blob storage to local storage with an optional parameter to write the local file as an append file. | 1,2,3 |

1 – HTTP REST API interface

2 – Edge Hub API interface

3 – Direct Methods interface



# Getting Started

The IoT Edge Log Module is simple to setup and use.  Follow this link for the Setup and Samples guides where we you how easy it can be to leverage persistent storage on your edge device and management it.

# Configuration

Task 1 – Define docker volume(s) on Edge Device

 Step 1: Logon edge device

 Step 2: Create a docker volume using the command _docker volume create \&lt;name\&gt;, e.g.,
             docker volume create data_

_       _ Step 3: Give read/write permission to docker volume

- Execute the following command _docker volume inspect \&lt;name\&gt;_
- Get the physical path of the docker volume
- Execute the following command s

_sudo chmod -R ugo+rw \&lt;physical path\&gt;_

Task 2 – Create Azure Storage Account

 Step 1: Create an Azure Storage Account if the portal

- Copy the name of the storage account, .e.g.. &quot;myteststore&quot;
- Copy the access key

Task 3 – Create IoT Hub and Edge Device

 Step 1: Go to Azure Portal and create an IoT Hub

 Step 2: Create an edge device in within the IoT Hub, e.g., &quot;edgedevice1&quot;

Task 4 – Add the Log Module to the Edge Device

 Step 1: Click on edge device in portal

 Step 2: Click &quot;Set Modules&quot;

 Step 3: Under &quot;Deployment Modules&quot; click &quot;+Add&quot; and select &quot;IoT Edge Module&quot;

 Step 4: Fill in the following information in the blade

- Name: iotedge-logmodule
- Image URI: skunklab/iotedge-logmodule
- Container Create Options paste the following, which assumes your docker volume created is name &quot;data&quot;, otherwise replace the highlight text with your docker volume name.

{
  &quot;ExposedPorts&quot;: {
    &quot;8877/tcp&quot;: {}
  },
  &quot;HostConfig&quot;: {
    &quot;Binds&quot;: [
      &quot;**data**:/app/**data**&quot;
    ],
    &quot;PortBindings&quot;: {
      &quot;8877/tcp&quot;: [
        {
          &quot;HostPort&quot;: &quot;8877&quot;
        }
   ]   }  } }

- Add the following Environment Variables

| **Name** | **Value** |
| --- | --- |
| LM\_Port | 8877 |
| LM\_BlobStorageAccountName | &lt;blob storage account name&gt; |
| LM\_BlobStorageAccountKey | &lt;blob storage access key&gt; |
| LM\_Features | WebHost;DirectMethodsHost |

- Click Save
- Click Next 2 times
- Click Submit



# Follow Up

The Log module is ready to be implemented.    You will want to call the log module from another module.  The [Nuget Package](https://www.nuget.org/packages/IoTEdge.LogModule.Core/0.9.8-prerelease) already has clients in C#, such that you can implement another module to test.  The following code can be used to write a file to the log module then upload to blob storage from another module.

## Write a file
```
public static async Task WriteNewFileAsync()

{
    string containerName = &quot;iotedge-logmodule&quot;;

    int port = 8877;

    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);

    string path = &quot;./data&quot;;

    string filename = &quot;test.txt&quot;;

    byte[] body = Encoding.UTF8.GetBytes(&quot;The quick brown fox jumps over the lazy dog.&quot;);

    bool append = false;

    await client.WriteFile(path, filename, body, append);

}
```

## Upload a file
```
public static async Task UploadFileAsync()

{

    string containerName = &quot;iotedge-logmodule&quot;;

    int port = 8877;

    HttpLogModuleClient client = new HttpLogModuleClient(containerName, port);

    string path = &quot;./data&quot;;

    string filename = &quot;test.txt&quot;;

    string blobPath = &quot;mycontainer&quot;;

    string blobFilename = &quot;mytest.txt&quot;;

    string contentType = &quot;text/plain&quot;;

    bool append = false;

   await client.UploadFile(path, filename, blobPath,

                           blobFilename, contentType, append);

}
