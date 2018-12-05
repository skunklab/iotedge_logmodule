
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
//using Microsoft.IdentityModel.Tokens;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Diagnostics;

namespace ProvisioningService
{

    public static class Provision
    {
        private static ServiceConfig config;
        private static string tokenserviceConnectionString;
        private static string rtuMapConnectionString;
        private static string piraeusApiToken;

        [FunctionName("Provision")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log, ExecutionContext context)
        {
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            config = new ServiceConfig();
            configuration.GetSection("ServiceConfig").Bind(config);

            tokenserviceConnectionString = configuration.GetConnectionString("TokenServiceConnectionString");
            rtuMapConnectionString = configuration.GetConnectionString("RtuMapConnectionString");

            log.Info("C# HTTP trigger function processed a request.");

            string luss = req.Query["luss"];

            //Procedure
            // (1) Use the LUSS to check for a valid field gateway 
            // (2) Get the token security to use with the Piraeus API
            // (3) Create the CAPL policies, which control access into and out-of the RTU
            // (4) Create the Piraeus resource for communication, which reference the CAPL policies
            // (5) Update the RTU-MAP for the Virtual RTU, which is needs recognize the new RTU
            // (6) Update the LUSS entry...it can never be used again even if the request has failed.
            // (7) Return the configuration for the field gateway to use (TokenParameters)




            Task<TokenParameters> task = ProvisionAsync(luss);
            Task.WaitAll(task);
            object result = task.Result;


            return result != null ? (ActionResult)new OkObjectResult(result) : new BadRequestObjectResult("Invalid LUSS.");
            
        }

        private static async Task<TokenParameters> ProvisionAsync(string luss)
        {


            if (string.IsNullOrEmpty(luss))
            {
                return null;
            }

            LussEntity entity = null;
            string securityToken = null;
            Tuple<string, string> tuple = null;
            TokenParameters parameters = null;

            try
            {
                entity = await LussEntity.LoadAsync(luss, config.LussTableName, tokenserviceConnectionString);
                if (entity == null || entity.Success.HasValue || entity.Expires < DateTime.Now)
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("LUSS entity '{0}' failed to load.", luss);
                Trace.TraceError(ex.Message);
                return null;
            }



            try
            {
                securityToken = GetSecurityToken(entity);
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Failed to issue security token with LUSS '{0}'.", luss);
                Trace.TraceError(ex.Message);
                entity.Success = false;
            }


            try
            {
                tuple = ProvisionPiraeus(entity);
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Failed to fully provision in Piraeus with LUSS '{0}'.", luss);
                Trace.TraceError(ex.Message);
                entity.Success = false;
            }

            try
            {
                UpdateRtuMap(entity, tuple.Item1, tuple.Item2, config.RtuMapContainerName, config.RtuMapFilename, rtuMapConnectionString);
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Failed to update RTU Map with LUSS '{0}'.", luss);
                Trace.TraceError(ex.Message);
                entity.Success = false;
            }

            try
            {
                Random ran = new Random();
                int index = ran.Next(0, config.PskIdentities.Length - 1);

                parameters = new TokenParameters()
                {
                    Hostname = config.PiraeusHostname,
                    Port = config.Port,
                    PskIdentity = config.PskIdentities[index],
                    PSK = config.Psks[index],
                    SecurityToken = securityToken,
                    Resources = new ResourceItem(tuple.Item1, tuple.Item2)
                };
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Failed to generate token parameters with LUSS '{0}'.", luss);
                Trace.TraceError(ex.Message);
            }


            entity.Access = DateTime.UtcNow;
            if (!entity.Success.HasValue)
            {
                entity.Success = true;
            }

            try
            {
                await entity.UpdateAsync(tokenserviceConnectionString);
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Failed to update LUSS table entry.");
                Trace.TraceError(ex.Message);
            }

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

        private static void UpdateRtuMap(LussEntity entity, string inboundResourceUriString, string outboundResourceUriString, string containerName, string filename, string connectionString)
        {
            RtuMap map = RtuMap.LoadFromConnectionString(containerName, filename, connectionString);
            if (!map.HasResources((ushort)entity.UnitId))
            {
                map.AddResource((ushort)entity.UnitId, inboundResourceUriString, outboundResourceUriString);
                Task task = map.UpdateMapAsync(containerName, filename, connectionString);
                Task.WhenAll(task);
            }
        }

        private static Tuple<string, string> ProvisionPiraeus(LussEntity entity)
        {
            string publishPolicyIdUriString = null;
            string subscribePolicyIdUriString = null;
            AuthorizationPolicy publishPolicy = CreateCaplPolicy(entity, true, out publishPolicyIdUriString);
            AuthorizationPolicy subscribePolicy = CreateCaplPolicy(entity, false, out subscribePolicyIdUriString);

            string inputResourceUriString = GetResourceMetadataUriString(entity, true);
            string outResourceUriString = GetResourceMetadataUriString(entity, false);

            ResourceMetadata inputMetadata = CreateResourceMetadata(inputResourceUriString, publishPolicyIdUriString, subscribePolicyIdUriString, (ushort)entity.UnitId, true);
            ResourceMetadata outputMetadata = CreateResourceMetadata(outResourceUriString, subscribePolicyIdUriString, publishPolicyIdUriString, (ushort)entity.UnitId, false);

            piraeusApiToken = GetInternalSecurityToken();


            SetCaplPolicy(publishPolicy);
            SetCaplPolicy(subscribePolicy);

            SetResourceMetadata(inputMetadata);
            SetResourceMetadata(outputMetadata);

            return new Tuple<string, string>(inputResourceUriString, outResourceUriString);

        }

        private static void SetCaplPolicy(AuthorizationPolicy policy)
        {

            string url = String.Format("http://{0}/api2/accesscontrol/upsertaccesscontrolpolicy", config.PiraeusApiHostname);
            RestRequestBuilder builder = new RestRequestBuilder("PUT", url, RestConstants.ContentType.Xml, false, piraeusApiToken);
            RestRequest request = new RestRequest(builder);

            request.Put<AuthorizationPolicy>(policy);



            //string url = String.Format("http://{0}/api2/accesscontrol/upsertaccesscontrolpolicy", config.PiraeusApiHostname);
            //RestRequestBuilder builder = new RestRequestBuilder("PUT", url, RestConstants.ContentType.Xml, false, config.PiraeusApiToken);
            //RestRequest request = new RestRequest(builder);

            //request.Put<AuthorizationPolicy>(policy);
        }


        private static void SetResourceMetadata(ResourceMetadata metadata)
        {
            string url = String.Format("http://{0}/api2/resource/upsertresourcemetadata", config.PiraeusApiHostname);
            RestRequestBuilder builder = new RestRequestBuilder("PUT", url, RestConstants.ContentType.Json, false, piraeusApiToken);
            RestRequest request = new RestRequest(builder);
            request.Put<ResourceMetadata>(metadata);
        }
        private static string GetResourceMetadataUriString(LussEntity entity, bool rtuInbound)
        {
            return rtuInbound ? String.Format("http://www.skunklab.io/{0}/unitid{1}-in", entity.VirtualRtuId, entity.UnitId) :
                                                    String.Format("http://www.skunklab.io/{0}/unitid{1}-out", entity.VirtualRtuId, entity.UnitId);
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
                            String.Format("http://www.skunklab.io/policy/{0}/unitid{1}-in", entity.VirtualRtuId, entity.UnitId) :
                            String.Format("http://www.skunklab.io/policy/{0}/unitid{1}-out", entity.VirtualRtuId, entity.UnitId);

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

        private static string GetInternalSecurityToken()
        {
            string url = String.Format("http://{0}/api3/manage?code={1}", config.PiraeusHostname, config.PiraeusApiToken);
            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, null);
            RestRequest request = new RestRequest(builder);

            return request.Get<string>();
        }
    }
}
