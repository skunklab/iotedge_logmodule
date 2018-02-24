using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Storage
{
    internal class Vault
    {
        public Vault(string vault, string clientId, string clientSecret, string keyName)
        {
            this.vault = vault;
            this.keyName = keyName;
            credential = new ClientCredential(clientId, clientSecret);
            client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAccessToken), GetHttpClient());
            secret = client.GetSecretAsync(keyName).GetAwaiter().GetResult();
            this.Key = secret.Value;
            resolver = new KeyVaultKeyResolver(client);
        }


        public Vault(string vault, string clientId, string clientSecret)
        {
            this.vault = vault;
            credential = new ClientCredential(clientId, clientSecret);
            client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAccessToken), GetHttpClient());            
            resolver = new KeyVaultKeyResolver(client);
        }

        private Secret secret;
        private ClientCredential credential;
        private KeyVaultKeyResolver resolver;
        private KeyVaultClient client;
        private string vault;
        private string keyName;

        public string GetKeyFromVault(string keyName)
        {
            secret = client.GetSecretAsync(keyName).GetAwaiter().GetResult();
            return secret.Value;
        }

        public string Key { get; internal set; }
                
        public BlobRequestOptions GetEncryptionBlobOptions(string keyName)
        {
            secret = client.GetSecretAsync(keyName).GetAwaiter().GetResult();
            var rsa = resolver.ResolveKeyAsync(String.Format("https://{0}.vault.azure.net/keys/{1}", this.vault, keyName), CancellationToken.None).GetAwaiter().GetResult();

            // Now you simply use the RSA key to encrypt by setting it in the BlobEncryptionPolicy.
            BlobEncryptionPolicy policy = new BlobEncryptionPolicy(rsa, null);
            return new BlobRequestOptions() { EncryptionPolicy = policy };
        }

        public TableRequestOptions GetEncryptionTableOptions(string keyName)
        {
            secret = client.GetSecretAsync(keyName).GetAwaiter().GetResult();
            var rsa = resolver.ResolveKeyAsync(String.Format("https://{0}.vault.azure.net/keys/{1}", this.vault, keyName), CancellationToken.None).GetAwaiter().GetResult();

            TableEncryptionPolicy policy = new TableEncryptionPolicy(rsa, null);
            return new TableRequestOptions() { EncryptionPolicy = policy };
        }

        public QueueRequestOptions GetEncryptionQueueOptions(string keyName)
        {
            secret = client.GetSecretAsync(keyName).GetAwaiter().GetResult();
            var rsa = resolver.ResolveKeyAsync(String.Format("https://{0}.vault.azure.net/keys/{1}", this.vault, keyName), CancellationToken.None).GetAwaiter().GetResult();

            QueueEncryptionPolicy policy = new QueueEncryptionPolicy(rsa, null);
            return new QueueRequestOptions() { EncryptionPolicy = policy };
        }


        public BlobRequestOptions GetEncryptionOptions()
        {
            // Retrieve the key that you created previously.
            // The IKey that is returned here is an RsaKey.
            // Remember that we used the names contosokeyvault and testrsakey1.
            var rsa = resolver.ResolveKeyAsync(String.Format("https://{0}.vault.azure.net/keys/{1}", this.vault, this.keyName), CancellationToken.None).GetAwaiter().GetResult();
                        
            // Now you simply use the RSA key to encrypt by setting it in the BlobEncryptionPolicy.
            BlobEncryptionPolicy policy = new BlobEncryptionPolicy(rsa, null);
            return new BlobRequestOptions() { EncryptionPolicy = policy };
        }

        private async Task<string> GetTokenAsync(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, credential);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }

        public async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, credential);

            return result.AccessToken;
        }

        private HttpClient GetHttpClient()
        {
            return (HttpClientFactory.Create(new InjectHostHeaderHttpMessageHandler()));
        }

    }
}
