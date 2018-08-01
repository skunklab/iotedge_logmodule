#IMPORTANT:  Import the Piraeus Powershell Module
import-module C:\_git\dev\piraeus_0.9.0_prerelease\src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Release\Piraeus.Module.dll




#Login to the Management API

#URL of the Piraeus Web Gateway
#If running in Azure use the hostname or IP address of the virtual machine
#If running locally, type "docker inspect webgateway" to obtain the IP address of the web gateway

$url = "http://piraeus.eastus.cloudapp.azure.com"  #Replace with Host name or IP address of the Piraeus Web Gateway
#$url = "http://localhost:1733"

#get a security token for the management API
$token = Get-PiraeusManagementToken -ServiceUrl $url -Key "12345678"

$account = "malong" #data lake store account name 
$domain = "microsoft.onmicrosoft.com" #AAD domain, e.g., microsoft.onmicrosoft.com
$resourceUri = "http://www.skunklab.io/resource-a"
$folderName = "test" #data lake store folder name
$appId = "0636d2b9-0593-4825-a9b0-381decd886e1" #app id created from AAD
$secret = "uwRORs3zhO2yOTau5FYNOUOReeDB/cqFyEidvXiZ+v8=" #client secret created from AAD


#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "SUBSCRIPTION_URI, e.g., http://www.skunklab.io/resource-a/34a8062c-4c7b-4cb8-a238-ecb4109941b5"

Add-PiraeusDataLakeSubscription -ServiceUrl $url -SecurityToken $token -Account $account -Domain "microsoft.onmicrosoft.com" -ResourceUriString $resourceUri -AppId $appId -ClientSecret $secret -Folder $folderName -NumClients 1