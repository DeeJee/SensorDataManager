using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorData.Api.Security
{
    public class AzureTokenHandler : AuthorizationHandler<AzureTokenRequirement>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration configuration;


        public AzureTokenHandler(IConfiguration configuration)
        {
            this.configuration = configuration;
            var section = configuration.GetValue<string>("AzureAD:ClientId");
        }
        //
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Tenant is the name of the tenant in which this application is registered.
        // The Authority is the sign-in URL of the tenant.
        // The Audience is the value the service expects to see in tokens that are addressed to it.
        //
        //private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];

            

        //private static string tenant = Configuration.ConfigurationManager.AppSettings["ida:Tenant"];
        //private static string audience = ConfigurationManager.AppSettings["ida:Audience"];
        //private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        //private string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        private static string _issuer = string.Empty;
        private static ICollection<SecurityKey> _signingKeys = null;
        private static DateTime _stsMetadataRetrievalTime = DateTime.MinValue;
        private static string scopeClaimType = "http://schemas.microsoft.com/identity/claims/scope";

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AzureTokenRequirement requirement)
        {
            throw new Exception();

            //// Get the jwt bearer token from the authorization header
            //string jwtToken = null;
            //AuthenticationHeaderValue authHeader = context.Request.Headers.Authorization;
            //if (authHeader == null || authHeader.Scheme.ToLower() != "bearer")
            //{
            //    logger.Error("No bearer token provided");
            //    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            //    return;
            //}
            //jwtToken = authHeader.Parameter;

            //string issuer;
            //ICollection<SecurityKey> signingKeys;

            //try
            //{
            //    // The issuer and signingKeys are cached for 24 hours. They are updated if any of the conditions in the if condition is true.
            //    if (DateTime.UtcNow.Subtract(_stsMetadataRetrievalTime).TotalHours > 24
            //        || string.IsNullOrEmpty(_issuer)
            //        || _signingKeys == null)
            //    {
            //        // Get tenant information that's used to validate incoming jwt tokens
            //        string stsDiscoveryEndpoint = $"{this.authority}/.well-known/openid-configuration";
            //        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            //        var config = configManager.GetConfigurationAsync().Result;
            //        _issuer = config.Issuer;
            //        _signingKeys = config.SigningKeys;

            //        _stsMetadataRetrievalTime = DateTime.UtcNow;
            //    }

            //    issuer = _issuer;
            //    signingKeys = _signingKeys;
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.Message);
            //    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            //    return;
            //}

            //JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            //TokenValidationParameters validationParameters = new TokenValidationParameters
            //{
            //    // We accept both the App Id URI and the AppId of this service application
            //    ValidAudiences = new[] { audience, clientId },

            //    // Supports both the Azure AD V1 and V2 endpoint
            //    ValidIssuers = new[] { issuer, $"{issuer}/v2.0" },
            //    IssuerSigningKeys = signingKeys
            //};

            //try
            //{
            //    // Validate token.
            //    SecurityToken validatedToken = new JwtSecurityToken();
            //    ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(jwtToken, validationParameters, out validatedToken);

            //    // Set the ClaimsPrincipal on the current thread.
            //    Thread.CurrentPrincipal = claimsPrincipal;

            //    // Set the ClaimsPrincipal on HttpContext.Current if the app is running in web hosted environment.
            //    if (HttpContext.Current != null)
            //    {
            //        HttpContext.Current.User = claimsPrincipal;
            //    }

            //    // If the token is scoped, verify that required permission is set in the scope claim.
            //    if (ClaimsPrincipal.Current.FindFirst(scopeClaimType) != null && ClaimsPrincipal.Current.FindFirst(scopeClaimType).Value != "user_impersonation")
            //    {
            //        actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            //        return;
            //    }

            //    return;
            //}
            //catch (SecurityTokenValidationException ex)
            //{
            //    logger.Error(ex.Message);
            //    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            //    return;
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.Message);
            //    actionContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            //    return;
            //}

            //var basic = actionContext.Request.Headers.Authorization.Parameter.Split(':');
            //var username = basic[0];
            //var password = basic[1];
            //if (!(username == "esp8266" && password == "489EACE8-BA68-481D-B2A5-A5AD9394B940"))
            //{
            //    logger.Warn($"Invalid username/password provided: {username}/{password} Login failed.");
            //    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            //}
        }
    }
}
