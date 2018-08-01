#Parameters
#ResourceUriString = Piraeus resource to subscribe
#Account = <account>.redis.cache.windows.net
#SecurityKey = Redis cache security key from Azure portal
#DatabaseNum = (Optional) Redis cache database number for cached items. If omitted uses the default database.
#Expiry = (Optional) TimeSpan expiration of cached items.  If omitted the cached item does not expire.
#ClaimType = (Optional) Used only when caching messages based on a unique claim type in the publisher's security token.
#                       If omitted, the publisher's can be a 'cache key' in the message parameters to specify a unique key.
#                       If ClaimType omitted AND does not send 'cache key', then message will not be cached.
#Description = (Optional) Text description on the subscription


#Example: Redis cache Subscription where the "ClaimType" defines a claim 
#in the caller's security token to use as a cache key. 
Add-PiraeusRedisCacheSubscription -ServiceUrl $url -SecurityToken $token `
                                  -ResourceUriString "" `
                                  -Account "" ` 
                                  -SecurityKey "" `
                                  -DatabaseNum 2 `
                                  -Expiry "" `
                                  -ClaimType "" `
                                  -Description ""

#Example: Redis cache Subscription where the "ClaimType" is omitted
#and Piraeus expects that the caller will supply a cache key.
Add-PiraeusRedisCacheSubscription -ServiceUrl $url -SecurityToken $token `
                                  -ResourceUriString "" `
                                  -Account "" `
                                  -SecurityKey "" `
                                  -DatabaseNum 2 `
                                  -Expiry "" `
                                  -Description ""




                                  