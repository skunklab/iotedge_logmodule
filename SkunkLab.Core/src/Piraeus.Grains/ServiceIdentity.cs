using Orleans;
using Orleans.Providers;
using Piraeus.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Piraeus.Grains
{
    [StorageProvider(ProviderName = "store")]
    public class ServiceIdentity : Grain<ServiceIdentityState>, IServiceIdentity
    {
        public override Task OnActivateAsync()
        {
            if (State.Claims != null || State.Certificate != null)
                return Task.CompletedTask;

            string claimTypes = System.Environment.GetEnvironmentVariable("ORLEANS_SERVICE_IDENTITY_CLAIM_TYPES");
            string claimValues = System.Environment.GetEnvironmentVariable("ORLEANS_SERVICE_IDENTITY_CLAIM_VALUES");
            string store = System.Environment.GetEnvironmentVariable("ORLEANS_X509_SERVICE_IDENTITY_STORE");
            string location = System.Environment.GetEnvironmentVariable("ORLEANS_X509_SERVICE_IDENTITY_LOCATION");
            string thumbprint = System.Environment.GetEnvironmentVariable("ORLEANS_X509_SERVICE_IDENTITY_THUMBPRINT");

            if(claimTypes != null && claimValues != null)
            {
                string[] ctypes = claimTypes.Split(new char[] { ';' });
                string[] cvalues = claimValues.Split(new char[] { ';' });

                if(ctypes.Length != cvalues.Length)
                {
                    Trace.TraceWarning("Service identity does not same number of claim types and values.");
                    return Task.CompletedTask;
                }

                State.Claims = new List<KeyValuePair<string,string>>();
                for(int i=0;i<ctypes.Length;i++)
                {
                    State.Claims.Add(new KeyValuePair<string,string>(ctypes[i], cvalues[i]));
                }
            }

            if(store != null && location != null && thumbprint != null)
            {
                //get the certificate from the store/location by thumbprint
                X509Certificate2 cert = GetLocalCertificate(store, location, thumbprint);
                if (cert != null)
                    State.Certificate = cert.RawData;
            }

            return Task.CompletedTask;
        }

        public override async Task OnDeactivateAsync()
        {
            await WriteStateAsync();
        }


        public Task<byte[]> GetCertificateAsync()
        {
            return Task.FromResult<byte[]>(State.Certificate);
        }

        public Task<List<KeyValuePair<string,string>>> GetClaimsAsync()
        {
            return Task.FromResult<List<KeyValuePair<string,string>>>(State.Claims);
        }

        public Task AddCertificateAsync(byte[] certificate)
        {
            State.Certificate = certificate;
            return WriteStateAsync();
        }

        public Task AddClaimsAsync(List<KeyValuePair<string,string>> claims)
        {            
            return Task.CompletedTask;
        }

        private X509Certificate2 GetLocalCertificate(string store, string location, string thumbprint)
        {
            if (string.IsNullOrEmpty(store) || string.IsNullOrEmpty(location) || string.IsNullOrEmpty(thumbprint))
            {
                return null;
            }


            thumbprint = Regex.Replace(thumbprint, @"[^\da-fA-F]", string.Empty).ToUpper();


            StoreName storeName = (StoreName)Enum.Parse(typeof(StoreName), store, true);
            StoreLocation storeLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), location, true);


            X509Store certStore = new X509Store(storeName, storeLocation);
            certStore.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection coll = certStore.Certificates;

            X509Certificate2Collection certCollection =
              certStore.Certificates.Find(X509FindType.FindByThumbprint,
                                      thumbprint.ToUpper(), false);
            X509Certificate2Enumerator enumerator = certCollection.GetEnumerator();
            X509Certificate2 cert = null;
            while (enumerator.MoveNext())
            {
                cert = enumerator.Current;
            }
            return cert;

        }
    }
}
