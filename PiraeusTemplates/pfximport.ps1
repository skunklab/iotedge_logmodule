
param([string]$pfxFilePath = "", [string]$pwd = "")
$mypwd = ConvertTo-SecureString -String $pwd -Force -AsPlainText
Import-PfxCertificate -FilePath $pfxFilePath cert:\localMachine\my -Password $mypwd



