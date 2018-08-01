#IMPORTANT:  Import the Piraeus Powershell Module
#import-module C:\_git\dev\piraeus_0.9.0_prerelease\src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Release\Piraeus.Module.dll

Import-Module Piraeus



#Login to the Management API

#URL of the Piraeus Web Gateway
#If running in Azure use the hostname or IP address of the virtual machine
#If running locally, type "docker inspect webgateway" to obtain the IP address of the web gateway

$url = "http://104.46.114.32"  #Replace with Host name or IP address of the Piraeus Web Gateway
#$url = "http://localhost:1733"

#get a security token for the management API
$token = Get-PiraeusManagementToken -ServiceUrl $url -Key "12345678"

$i = 0
DO
{
    $account = "malong" #data lake store account name 
    $domain = "microsoft.onmicrosoft.com" #AAD domain, e.g., microsoft.onmicrosoft.com
    $resourceUri = "http://www.skunklab.io/resource-" + $i
    $folderName = "test-" + $i #data lake store folder name
    $appId = "0636d2b9-0593-4825-a9b0-381decd886e1" #app id created from AAD
    $secret = "uwRORs3zhO2yOTau5FYNOUOReeDB/cqFyEidvXiZ+v8=" #client secret created from AAD

    Add-PiraeusDataLakeSubscription -ServiceUrl $url -SecurityToken $token -Account $account -Domain "microsoft.onmicrosoft.com" -ResourceUriString $resourceUri -AppId $appId -ClientSecret $secret -Folder $folderName -NumClients 1 -Description "Data Lake"

    $i++

}While($i -le 9)










#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-0/93a67e86-e88b-4ef3-8189-4567aaf188e2"
#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-1/3e859d13-b8d9-4951-81bc-69df3f7e09f3"
#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-2/14d54484-78c2-4a3f-a018-e10940a95550"
#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-3/f4e2cf32-b0a5-4cd4-975f-e28bdd27f490"
#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-4/6f4de6fe-f33d-429d-b444-59908d796f88"
#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-5/ae735058-e900-423a-96c3-cad17194a6ec"
#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-6/3d094f72-80b5-4a52-89bc-a6f1f2370680"
#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-7/b785a4b5-a76d-428e-81f0-7cf8e28dd986"
#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-8/d1b1e584-1565-491e-8e76-4a914eccfe52"
#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-9/d52fe288-0b38-4776-a9e0-c5f693baf5dc"



#Get-PiraeusSubscriptionList -ServiceUrl $url -SecurityToken $token -ResourceUriString "http://www.skunklab.io/resource-0" | Write-Output

#Remove-PiraeusSubscription -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-9/1005c584-7a42-4761-83fb-3f51b5e475b3"

#$meta1 = Get-PiraeusSubscriptionMetadata -ServiceUrl $url -SecurityToken $token -SubscriptionUriString $list[1]
#$meta2 = Get-PiraeusSubscriptionMetadata -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-8/7ddee329-41bc-4ce4-9af8-3350bec6d1b9"
#$meta3 = Get-PiraeusSubscriptionMetadata -ServiceUrl $url -SecurityToken $token -SubscriptionUriString "http://www.skunklab.io/resource-8/9614ce6f-8dcb-493a-a038-786d508c60ad"

#$meta1
#$meta2
#$meta3
#http://www.skunklab.io/resource-0/8ae7384c-40fc-4c44-a79b-44bc82c05e64


#Add-PiraeusEventHubSubscription -ServiceUrl $url -SecurityToken $token -ResourceUriString $resourceUri -Account "piraeus" -Hub "piraeushub" -KeyName "RootManageSharedAccessKey" -Key "HFwNifJH8qjFXHUt76fq++73OeG3aToozMHjmJN+hJ0=" -NumClients 10 -Description "Event Hub"

