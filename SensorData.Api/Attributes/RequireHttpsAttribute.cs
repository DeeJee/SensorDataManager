using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SensorDataApi.Attributes
{
    public class RequireHttpsAttribute : AuthorizationFilterAttribute
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                Log.Info($"Illegal http request detected: {actionContext.Request.RequestUri.AbsoluteUri}");
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Found);
                actionContext.Response.Content = new StringContent("use https instead of http");

                UriBuilder builder = new UriBuilder(actionContext.Request.RequestUri);
                builder.Scheme = Uri.UriSchemeHttps;
#if DEBUG
                builder.Port = 44374;
#endif

                actionContext.Response.Headers.Location = builder.Uri;
                Log.Info($"Redirecting to {actionContext.Response.Headers.Location.AbsoluteUri}");
            }
            else
            {
                base.OnAuthorization(actionContext);
            }
        }
    }
}
