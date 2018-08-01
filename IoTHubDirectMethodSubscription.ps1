#IoT Hub Direct Method Subscription
#Note: Makes a direct method call to a configured device.

#ResourceUriString = Piraeus resource to subscribe
#Account = <account>.azure-devices.net
#DeviceId = Device ID to send message
#Method = Direct method name
#KeyName = Key name for direct method to device
#Key = Security key associated with key name
#Description = (Optional) Text description on the subscription

$resource = ""
$account = ""
$deviceId = ""
$method = ""
$keyName = ""
$key = ""
$description = ""

Add-PiraeusIotHubDirectMethodSubscription -ServiceUrl $url -SecurityToken $token `
                                           -ResourceUriString $resource `
                                           -Account $account `
                                           -DeviceId $deviceId `
                                           -Method $method `
                                           -KeyName $keyName `
                                           -Key $key `
                                           -Description $description