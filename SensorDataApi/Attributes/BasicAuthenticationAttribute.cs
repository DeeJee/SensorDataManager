using NLog;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SensorDataApi.Attributes
{
    public class BasicAuthenticationAttribute : AuthorizationFilterAttribute
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override void OnAuthorization(HttpActionContext actionContext)
        {

            if (actionContext.Request.Headers.Authorization == null)
            {
                logger.Warn("No authorization provided. Login failed.");
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            else
            {
                var basic = actionContext.Request.Headers.Authorization.Parameter.Split(':');
                var username = basic[0];
                var password = basic[1];
                if (!(username == "esp8266" && password == "489EACE8-BA68-481D-B2A5-A5AD9394B940"))
                {
                    logger.Warn($"Invalid username/password provided: {username}/{password} Login failed.");
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
        }
    }
}
