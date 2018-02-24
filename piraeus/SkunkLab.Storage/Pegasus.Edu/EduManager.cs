using Capl.Authorization;
using SkunkLab.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Pegasus.Edu
{
    public class EduManager
    {
        public static string ProjectTableName { get; set; }

        public static string AccessControlTableName { get; set; }
        public static string ConnectionString { get; set; }
        public static void AddProject(ProjectEntity entity)
        {
            if(!IsUniqueProject(entity))
            {
                RegenCode(entity);
            }

            TableStorage storage = TableStorage.CreateSingleton(ConnectionString);
            storage.Write(ProjectTableName, entity);
        }

        public static void AddResources(ProjectEntity entity)
        {
            string telemetryResource = String.Format("http://pegasusmission.edu/{0}{1}", entity.Code, "telemetry");
            string groundResource = String.Format("http://pegasusmission.edu/{0}{1}", entity.Code, "ground");


            string gatewayResource = String.Format("http://pegasusmission.edu/{0}{1}", entity.Code, "gateway");
            string clientResource = String.Format("http://pegasusmission.edu/{0}{1}", entity.Code, "client");
            //add the resources to the Piraeus REST API
            //TODO: ...

            //send ground and craft telemetry
            AuthorizationPolicy telemetryPolicy = AccessControl.GetPolicy("http://pegasusmission.edu/gateway", "http://pegasusmission.edu/role", String.Format("{0}{1}", entity.Code, "gateway"));
            //receive ground and craft telemetry
            AuthorizationPolicy clientPolicy = AccessControl.GetPolicy("", "http://pegasusmission.edu/role", String.Format("{0}{1}", entity.Code, "client"));

            AddAccessControl(entity.Code, entity.Name, entity.School, telemetryPolicy);
            AddAccessControl(entity.Code, entity.Name, entity.School, clientPolicy);             
        }

        public static void AddAccessControl(string code, string name, string school, AuthorizationPolicy policy)
        {
            //add the access control policy to the table 
            XmlWriterSettings settings = new XmlWriterSettings() { OmitXmlDeclaration = true };
            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                policy.WriteXml(writer);
                writer.Flush();
                writer.Close();
            }

            AccessControlEntity entity = new AccessControlEntity(code, name, school, builder.ToString());
            TableStorage storage = TableStorage.CreateSingleton(AccessControlTableName);
            storage.Write(ConnectionString, entity);     
        }

        private static void RegenCode(ProjectEntity entity)
        {
            Random ran = new Random();
            int code = ran.Next(100000, 999999);
            entity.Code = code.ToString();

            if(!IsUniqueProject(entity))
            {
                RegenCode(entity);
            }
        }

        private static bool IsUniqueProject(ProjectEntity entity)
        {

            TableStorage storage = TableStorage.CreateSingleton(ConnectionString);
            List<ProjectEntity> list = storage.Read<ProjectEntity>(ProjectTableName, entity.PartitionKey);
            return (list == null || list.Count == 0);
        }
    }
}
