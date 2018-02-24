param([string]$store1, [string]$key1, [string]$store2, [string]$key2)

$connectionstring1 = "DefaultEndpointsProtocol=https;AccountName=" + $store1 + ";AccountKey=" + $key1 + ";EndpointSuffix=core.windows.net"
$connectionstring2 = "DefaultEndpointsProtocol=https;AccountName=" + $store2 + ";AccountKey=" + $key2 + ";EndpointSuffix=core.windows.net"

Write-Host $connectionstring1
Write-Host $connectionstring2