using Capl.Authorization;
using Capl.Authorization.Matching;
using Capl.Authorization.Operations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace TokenService
{
    public static class Function1
    {

        private static ServiceConfig config;
        private static string tokenserviceConnectionString;
        private static string rtuMapConnectionString;

        [FunctionName("TokenService")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log, ExecutionContext context)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()                
                .Build();

            config = configuration.Get<ServiceConfig>();
            tokenserviceConnectionString = configuration.GetConnectionString("TokenServiceConnectionString");
            rtuMapConnectionString = configuration.GetConnectionString("RtuMapConnectionString");




            log.Info("C# HTTP trigger function processed a request.");
            
            string luss = req.Query["luss"];
            Task<TokenParameters> task = ProvisionAsync(luss);
            Task.WaitAll(task);
            object result = task.Result;



            return result != null ? (ActionResult)new OkObjectResult(result) : new BadRequestObjectResult("Invalid LUSS.");
        }

        

        private static async Task<TokenParameters> ProvisionAsync(string luss)
        {
            if(string.IsNullOrEmpty(luss))
            {
                return null;
            }

            LussEntity entity = await LussEntity.LoadAsync(luss, tokenserviceConnectionString);
            if(entity == null || !entity.Completed.HasValue)
            {
                return null;
            }

            if (DateTime.UtcNow.Subtract(entity.Created).TotalHours > 1.0)
            {
                return null;
            }

            string securityToken = GetSecurityToken(entity);
            Tuple<string,string> tuple = ProvisionPiraeus(entity);
            UpdateRtuMap(entity, tuple.Item1, tuple.Item2,rtuMapConnectionString);

            entity.Completed = DateTime.UtcNow;
            entity.Issued = DateTime.UtcNow;

            Random ran = new Random();
            int index = ran.Next(0, config.PskIdentities.Length);



            TokenParameters parameters = new TokenParameters()
            {
                Hostname = config.PiraeusHostname,
                Port = 8883,
                PskIdentity = config.PskIdentities[index],
                PSK = config.Psks[index],
                SecurityToken = securityToken,
                Resources = new ResourceItem(tuple.Item1, tuple.Item2)
            };

            return parameters;
        }

        private static string GetSecurityToken(LussEntity entity)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(config.NameClaimType, String.Format("fieldgateway{0}", entity.UnitId)));
            claims.Add(new Claim(config.RoleClaimType, String.Format("device")));

            JsonWebToken jwt = new JsonWebToken(config.SymmetricKey, claims, Convert.ToDouble(config.LifetimeMinutes), config.Issuer, config.Audience);
            return jwt.ToString();
        }

        private static void UpdateRtuMap(LussEntity entity, string inboundResourceUriString, string outboundResourceUriString, string connectionString)
        {
            RtuMap map = RtuMap.LoadFromEnvironmentVariable();
            if(!map.HasResources(entity.UnitId))
            {
                map.AddResource(entity.UnitId, inboundResourceUriString, outboundResourceUriString);
                Task task = map.UpdateMapAsync(connectionString);
                Task.WhenAll(task);
            }
        }

        private static Tuple<string,string> ProvisionPiraeus(LussEntity entity)
        {
            string publishPolicyIdUriString = null;
            string subscribePolicyIdUriString = null;
            AuthorizationPolicy publishPolicy = CreateCaplPolicy(entity, true, out publishPolicyIdUriString);
            AuthorizationPolicy subscribePolicy = CreateCaplPolicy(entity, false, out subscribePolicyIdUriString);

            string inputResourceUriString = GetResourceMetadataUriString(entity, true);
            string outResourceUriString = GetResourceMetadataUriString(entity, false);

            ResourceMetadata inputMetadata = CreateResourceMetadata(inputResourceUriString, publishPolicyIdUriString, subscribePolicyIdUriString, entity.UnitId, true);
            ResourceMetadata outputMetadata = CreateResourceMetadata(outResourceUriString, subscribePolicyIdUriString, publishPolicyIdUriString, entity.UnitId, false);

            SetCaplPolicy(publishPolicy);
            SetCaplPolicy(subscribePolicy);

            SetResourceMetadata(inputMetadata);
            SetResourceMetadata(outputMetadata);

            return new Tuple<string, string>(inputResourceUriString, outResourceUriString);
            
        }

        private static void SetCaplPolicy(AuthorizationPolicy policy)
        {
            string url = String.Format("{0}/api2/accesscontrol/upsertaccesscontrolpolicy", config.PiraeusApiHostname);
            RestRequestBuilder builder = new RestRequestBuilder("PUT", url, RestConstants.ContentType.Xml, false, config.PiraeusApiToken);
            RestRequest request = new RestRequest(builder);

            request.Put<AuthorizationPolicy>(policy);
        }


        private static void SetResourceMetadata(ResourceMetadata metadata)
        {
            string url = String.Format("{0}/api2/resource/upsertresourcemetadata", config.PiraeusApiHostname);
            RestRequestBuilder builder = new RestRequestBuilder("PUT", url, RestConstants.ContentType.Json, false, config.PiraeusApiToken);
            RestRequest request = new RestRequest(builder);
            request.Put<ResourceMetadata>(metadata);
        }



        private static string GetResourceMetadataUriString(LussEntity entity, bool rtuInbound)
        {
            return rtuInbound ? String.Format("http://www.skunklab.io/{0}/unitid{1}-in", entity.GroupId, entity.UnitId) :
                                                    String.Format("http://www.skunklab.io/{0}/unitid{1}-out", entity.GroupId, entity.UnitId);
        }

        private static ResourceMetadata CreateResourceMetadata(string resourceUriString, string publishPolicyIdUriString, string subscribePolicyIdUriString, ushort unitId, bool inboundRtu)
        {
            return new ResourceMetadata()
            {
                Audit = true,
                Description = inboundRtu ? String.Format("RTU {0} input resource.", unitId) : String.Format("RTU {0} output resource.", unitId),
                Enabled = true,
                RequireEncryptedChannel = true,
                ResourceUriString = resourceUriString,
                PublishPolicyUriString = publishPolicyIdUriString,
                SubscribePolicyUriString = subscribePolicyIdUriString
            };
        }


        private static AuthorizationPolicy CreateCaplPolicy(LussEntity entity, bool publishPolicy, out string policyIdUriString)
        {
            string policyId = publishPolicy ? 
                            String.Format("http://www.skunklab.io/policy/{0}/unitid{1}-in", entity.GroupId, entity.UnitId) : 
                            String.Format("http://www.skunklab.io/policy/{0}/unitid{1}-out", entity.GroupId, entity.UnitId);

            string claimType = publishPolicy ? config.RoleClaimType : config.NameClaimType;
            string claimValue = publishPolicy ? "vrtu" : String.Format("fieldgateway{0}", entity.UnitId);            

            policyIdUriString = policyId;

            return GetPolicy(policyId, claimType, claimValue);
        }


        private static string SerializeResourceMetadata(ResourceMetadata metadata)
        {
            return JsonConvert.SerializeObject(metadata);
        }

        private static string SerializePolicy(AuthorizationPolicy policy)
        {
            XmlWriterSettings settings = new XmlWriterSettings() { OmitXmlDeclaration = true };
            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                policy.WriteXml(writer);
                writer.Flush();
                writer.Close();
            }

            return builder.ToString();
        }


        private static AuthorizationPolicy GetPolicy(string policyIdUriString, string matchClaimType, string matchClaimValue)
        {
            Match match = new Match(LiteralMatchExpression.MatchUri, matchClaimType, true);
            EvaluationOperation equalOperation = new EvaluationOperation(EqualOperation.OperationUri, matchClaimValue);
            Rule rule = new Rule(match, equalOperation, true);
            return new AuthorizationPolicy(rule, new Uri(policyIdUriString));
        }
    }
}
