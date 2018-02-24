
#Configure a Storage Account to Receive messages


#IMPORTANT:  Import the Piraeus Powershell Module
import-module c:\_git\core\src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Release\Piraeus.Module.dll

#Login to the Management API

#URL of the Piraeus Web Gateway
#If running in Azure use the hostname or IP address of the virtual machine
#If running locally, type "docker inspect webgateway" to obtain the IP address of the web gateway

$url = "http://52.184.167.66"  #Replace with Host name or IP address of the Piraeus Web Gateway


#get a security token for the management API
$token = Get-PiraeusManagementToken -ServiceUrl $url -Key "12345678"


$resource_A = "http://www.skunklab.io/resource-a"

$hostname="pirstore"  #If the blob storage endpint is "https://piraeusstore.blob.core.windows.net/" use "piraeusstore" as the hostname
$containerName="resource-a"
$key="p6cq1r9wNqfBuK6IjVQkUJW1WzlKAcraAII/g2PMVkSxT3jhR8eMe8eDQtZc8L0VWXY3p88KRD6Kh57Zw+G/oQ=="

Add-PiraeusBlobStorageSubscription -ServiceUrl $url -SecurityToken $token -ResourceUriString $resource_A  -BlobType Block -Host $hostname -Container $containerName -Key $key 
