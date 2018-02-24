Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Force
New-NetFirewallRule -Name "CoAP UDP" -DisplayName "CoAP UDP" -Group Piraeus -Enabled True -Direction Inbound -Protocol UDP -RemotePort 5683 -LocalPort 5683
New-NetFirewallRule -Name "CoAP TCP" -DisplayName "CoAP TCP" -Group Piraues -Enabled True -Direction Inbound -Protocol TCP -RemotePort 5684 -LocalPort 5684
New-NetFirewallRule -Name "MQTT TCP not secure" -DisplayName "MQTT TCP not secure" -Group Piraues -Enabled True -Direction Inbound -Protocol TCP -RemotePort 1883 -LocalPort 1883
New-NetFirewallRule -Name "MQTT TCP secure" -DisplayName "MQTT TCP secure" -Group Piraues -Enabled True -Direction Inbound -Protocol TCP -RemotePort 8883 -LocalPort 8883
New-NetFirewallRule -Name "MQTT UDP" -DisplayName "MQTT UDP" -Group Piraues -Enabled True -Direction Inbound -Protocol TCP -RemotePort 5883 -LocalPort 5883


