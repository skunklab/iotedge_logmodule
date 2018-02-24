param([string]$cs1, [string]$cs2)

$path = "docker-compose.yml"
(Get-Content $path) -replace "#ORLEANS_BLOB_STORAGE_CONNECTIONSTRING",$cs1 | out-file $path 
(Get-Content $path) -replace "#AUDIT_BLOB_STORAGE_CONNECTIONSTRING",$cs2 | out-file $path 


