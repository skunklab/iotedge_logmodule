param([string]$store1, [string]$key1, [string]$store2, [string]$key2)

# 1.  Install Azure Powershell Module
# 2.  Create c:\piraeus folder
# 3.  Set ACL on folder to allow 'Authenticated Users' (needed for docker volumes)
# 4.  Download and install docker compose
# 5.  Download docker-compose YAML file
# 6.  Download docker environment variables file
# 7.  Update environment variables file for Orleans grain store (storage acct + key)
# 8.  Update environment variables file for sample storage acct (storage acct + key)
# 9.  Install External Virtual Switch in Hyper-V host
#10.  Pull the 3 Piraeus docker images
#11.  Run Docker Compose
#12.  Restart the VM (require to set Docker with the External Virtual Switch)

#Note:  The Web gateway image is large; making the initial deployment 15-22 minutes.  
#Note:  When the VM is running you can open a browser go to http://ipaddress of the VM.
#       Check the "running" indicator on the weg gateway's home page
#       You are ready to run the sample!

#Dare Mighty Things :-)


#Install the Azure Powershell Module 
Set-PSRepository -Name PSGallery -InstallationPolicy Trusted
Install-Module AzureRM -AllowClobber -Force
Import-Module AzureRM

#create a folder for files
cd \
mkdir piraeus
cd piraeus




#yml file location
$ymlFileUrl = "https://raw.githubusercontent.com/skunklab/piraeus_0.9.0_prerelease/master/src/Docker/docker-compose-azure.yml"

#env file location
$envFileUrl = "https://raw.githubusercontent.com/skunklab/piraeus_0.9.0_prerelease/master/src/Docker/gateway-config.env"


#Download docker-compose.yml file
Invoke-WebRequest -Uri $ymlFileUrl -UseBasicParsing -OutFile "docker-compose.yml" 

#Download gateway-config.env file
Invoke-WebRequest -Uri $envFileUrl -UseBasicParsing -OutFile "gateway-config.env" 



function UpdateYmlAndStore
{
    Param ([string]$acctName, [string]$storeKey, [string]$matchString, $containerName)

    $connectionString = "DefaultEndpointsProtocol=https;AccountName=" + $acctName + ";AccountKey=" + $storeKey

    $path = "docker-compose.yml"

    (Get-Content $path) -replace $matchString,$connectionString | out-file $path

    $context = New-AzureStorageContext -StorageAccountName $acctName -StorageAccountKey $storeKey -Protocol Https
    New-AzureStorageContainer -Name $containerName -Context $context
}


#Add the storage account container for orleans grain state
UpdateYmlAndStore -acctName $store1 -storeKey $key1 -matchString "#ORLEANS_BLOB_STORAGE_CONNECTIONSTRING" -containerName "orleans"

#Add the storage account container for the sample
UpdateYmlAndStore -acctName $store2 -storeKey $key2 -matchString "#AUDIT_BLOB_STORAGE_CONNECTIONSTRING" -containerName "resource-a"
