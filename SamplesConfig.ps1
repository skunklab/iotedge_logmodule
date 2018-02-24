
#IMPORTANT:  Import the Piraeus Powershell Module
Import-Module "c:\_git\core\src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Release\Piraeus.Module.dll"

#Login to the Management API

#URL of the Piraeus Web Gateway
#If running in Azure use the hostname or IP address of the virtual machine
#If running locally, type "docker inspect webgateway" to obtain the IP address of the web gateway

$url = "http://13.68.99.161"  #Replace with Host name or IP address of the Piraeus Web Gateway


#get a security token for the management API
$token = Get-PiraeusManagementToken -ServiceUrl $url -Key "12345678"



#The client demos create security tokens based on the selection of a "Role", i.e., "A" or "B"
#We will 2 CAPL policies 
# (1) a user in role "A" may transmit to 'resource-a' and subscribe to 'resource-b'
# (2) a user in role "B" may transmit to 'resource-b' and subscribe to 'resource-a'


# -- Building CAPL Authorization Policies ---
# (1) Match Expression -- find a claim type in the security token
# (2) Operation -- bind the claim value from the matched claim type and perform and operation, e.g., Equals
# (3) Rule -- create a rule that binds a match expression and an operation
# (4) Policy -- create a policy that is uniquely identifiable, that incorporates a Rule (or Logical Connective)


#--------------------------------------------------------------------------
#--------------- CAPL policy for users in role "A" ------------------------

#define the claim type to match to determines the client's role
$matchClaimType = "http://www.skunklab.io/role"

#create a match expression of 'Literal' to match the role claim type
$match = New-CaplMatch -Type Literal -ClaimType $matchClaimType -Required $true  

#create an operation to check the match claim value is 'Equal' to "A"
$operation_A = New-CaplOperation -Type Equal -Value "A"

#create a rule to bind the match expression and operation
$rule_A = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation_A

#define a unique identifier (as URI) for the policy
$policyId_A = "http://www.skunklab.io/resource-a" 

#create the policy for users in role "A"
$policy_A = New-CaplPolicy -PolicyID $policyId_A -EvaluationExpression $rule_A

#-------------------End Policy for "A"-------------------------------------



#--------------------------------------------------------------------------
#--------------- CAPL policy for users in role "B" ------------------------

#create an operation to check the match claim value is 'Equal' to "B"
$operation_B = New-CaplOperation -Type Equal -Value "B"

#create a rule to bind the match expression and operation
$rule_B = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation_B

#define a unique identifier (as URI) for the policy
$policyId_B = "http://www.skunklab.io/resource-b" 

#create the policy for users in role "A"
$policy_B = New-CaplPolicy -PolicyID $policyId_B -EvaluationExpression $rule_B

#-------------------End Policy for "B"------------------------------------


#--------------------------------------------------------------------------
# The policies are completed.  We need to add them to Piraeus

Add-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $policy_A 
Add-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $policy_B

#-------------------End adding policies to Piraeus-------------------------


#--------------------------------------------------------------------------
# We need to create the Piraeus resources and reference the CAPL polices
# in the resource metadata.  This providers access control for send or
# subscribing to a resource.
#--------------------------------------------------------------------------


#Uniquely identify Piraeus resources by URI
$resource_A = "http://www.skunklab.io/resource-a"
$resource_B = "http://www.skunklab.io/resource-b"

#Add the resources to Piraeus

#Resource "A" lets users with role "A" send and users with role "B" subscribe to receive transmissions
Add-PiraeusResourceMetadata -ResourceUriString $resource_A -Enabled $true -RequireEncryptedChannel $false -PublishPolicyUriString $policyId_A -SubscribePolicyUriString $policyId_B -ServiceUrl $url -SecurityToken $token 

#Resource "B" lets users with role "B" send and users with role "A" subscribe to receive transmissions
Add-PiraeusResourceMetadata -ResourceUriString $resource_B -Enabled $true -RequireEncryptedChannel $false -PublishPolicyUriString $policyId_B -SubscribePolicyUriString $policyId_A -ServiceUrl $url -SecurityToken $token


#Quick check get the resource data and verify what was set
$data1 = ""
$data2 = ""
$data1 = Get-PiraeusResourceMetadata -ResourceUriString $resource_A -ServiceUrl $url -SecurityToken $token
$data2 = Get-PiraeusResourceMetadata -ResourceUriString $resource_B -ServiceUrl $url -SecurityToken $token
