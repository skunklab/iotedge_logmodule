New-NetFirewallRule -Name "CoAP UDP" -DisplayName "CoAP UDP" -Group Piraeus -Enabled True -Direction Inbound -Protocol UDP -RemotePort 5683 -LocalPort 5683
New-NetFirewallRule -Name "CoAP TCP" -DisplayName "CoAP TCP" -Group Piraues -Enabled True -Direction Inbound -Protocol TCP -RemotePort 5684 -LocalPort 5684
New-NetFirewallRule -Name "MQTT TCP not secure" -DisplayName "MQTT TCP not secure" -Group Piraues -Enabled True -Direction Inbound -Protocol TCP -RemotePort 1883 -LocalPort 1883
New-NetFirewallRule -Name "MQTT TCP secure" -DisplayName "MQTT TCP secure" -Group Piraues -Enabled True -Direction Inbound -Protocol TCP -RemotePort 8883 -LocalPort 8883
New-NetFirewallRule -Name "MQTT UDP" -DisplayName "MQTT UDP" -Group Piraues -Enabled True -Direction Inbound -Protocol TCP -RemotePort 5883 -LocalPort 5883
New-NetFirewallRule -Name "Orleans Gateway" -DisplayName "Orleans Gateway" -Group Piraeus -Enabled True -Direction Outbound -Protocol TCP -RemotePort 11111
cd\
iwr https://github.com/skunklab/core2/raw/master/templates/MicrosoftAzureStorageTools.msi -OutFile "c:\MicrosoftAzureStorageTools.msi"
cd\
mkdir WebDeployment
iwr https://github.com/skunklab/core2/raw/master/templates/TestWebApp.deploy.cmd -OutFile "c:\WebDeployment\TestWebApp.deploy.cmd"
iwr https://github.com/skunklab/core2/raw/master/templates/TestWebApp.zip -OutFile "c:\WebDeployment\TestWebApp.zip"
iwr https://chocolatey.org/install.ps1 -UseBasicParsing | iex
iwr https://github.com/skunklab/core2/raw/master/templates/SL-YAMS.0.1.0.nupkg -OutFile "c:\SL-YAMS.0.1.0.nupkg"
choco install SL-YAMS -source "c:\SL-YAMS.0.1.0.nupkg"
Msiexec.exe /i c:\MicrosoftAzureStorageTools.msi /quiet
cd WebDeployment
./TestWebApp.deploy.cmd /Y
