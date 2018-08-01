using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Security.Identity
{
    public class IdentityDecoder
    {
        /// <summary>
        /// Decodes the claims identity
        /// </summary>
        /// <param name="identityClaimType">Claim type of the unique identifier of the identity.</param>
        /// <param name="indexes">Key value pair of claim type and index name.</param> 
        public IdentityDecoder(string identityClaimType, List<KeyValuePair<string,string>> indexes = null)
        {
            Id = DecodeClaimType(identityClaimType);

            if (indexes != null)
            {
                Indexes = new List<KeyValuePair<string, string>>();

                foreach (var item in indexes)
                {
                    string value = DecodeClaimType(item.Key);
                    if (!string.IsNullOrEmpty(value))
                    {
                        Indexes.Add(new KeyValuePair<string, string>(item.Value, value));
                    }
                }
            }
        }

        public string Id { get; internal set; }

        public List<KeyValuePair<string,string>> Indexes { get; internal set; }

        private string DecodeClaimType(string claimType)
        {
            Task<string> task = Task.Factory.StartNew(() =>
            {
                if (claimType == null)
                {
                    return null;
                }

                var principal = Thread.CurrentPrincipal as ClaimsPrincipal;
                if (principal == null)
                {
                    return null;
                }

                var identity = new ClaimsIdentity(principal.Claims);
                Claim claim =
                    identity.FindFirst(
                        c =>
                            c.Type.ToLower(CultureInfo.InvariantCulture) ==
                            claimType.ToLower(CultureInfo.InvariantCulture));

                return claim == null ? null : claim.Value;
            });

            return task.Result;
        }
    }
}
