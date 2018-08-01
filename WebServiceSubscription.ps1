

#Add REST Web Service Subscription with JWT security token

Add-PiraeusWebServiceSubscription -ServiceUrl $url -SecurityToken $token `
                                  -ResourceUriString "" `
                                  -WebServiceUrl "" `
                                  -Issuer "" `
                                  -Audience "" `
                                  -TokenType Jwt `
                                  -Key "" `
                                  -Description "REST Web Service Subscription"

#Add Azure Function Subscription
Add-PiraeusWebServiceSubscription -ServiceUrl $url -SecurityToken $token `
                                  -ResourceUriString "" `
                                  -WebServiceUrl "" `                                 
                                  -TokenType None `
                                  -Description "Azure Function Subscription"