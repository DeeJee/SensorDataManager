using System;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;

namespace SensorData.Api.Security
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            //var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            //var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            var credentials = authHeader.Parameter.Split(new[] { ':' }, 2);
            var username = credentials[0];
            var password = credentials[1];
            if (!(username == "esp8266" && password == "489EACE8-BA68-481D-B2A5-A5AD9394B940"))
            {
                logger.Warn($"Invalid username/password provided: {username}/{password} Login failed.");
                return AuthenticateResult.Fail("Invalid Username or Password");
            }

            var claims = new[] {
                new Claim(ClaimTypes.Name, "esp8266"),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
