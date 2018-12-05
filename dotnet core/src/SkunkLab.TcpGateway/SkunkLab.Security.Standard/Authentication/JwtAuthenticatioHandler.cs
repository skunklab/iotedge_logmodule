using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SkunkLab.Security.Tokens;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Security.Authentication
{
    public class JwtAuthenticatioHandler : AuthenticationHandler<JwtAuthenticationOptions>
    {
        protected JwtAuthenticatioHandler(IOptionsMonitor<JwtAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorization))
            {
                return Task.FromResult(AuthenticateResult.Fail("Cannot read authorization header."));
            }

            string[] parts = authorization.ToArray()[0].Split(" ");
            if(parts.Length != 2)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid JWT security token."));
            }

            string tokenString = parts[1];
            try
            {
                JsonWebToken.Authenticate(tokenString, Options.Issuer, Options.Audience, Options.SigningKey);
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.Message);
                return Task.FromResult(AuthenticateResult.Fail("Not authenticated."));
            }

            var ticket = new AuthenticationTicket((ClaimsPrincipal)Thread.CurrentPrincipal, Options.Scheme);
            return Task.FromResult<AuthenticateResult>(AuthenticateResult.Success(ticket));
        }
    }
}
