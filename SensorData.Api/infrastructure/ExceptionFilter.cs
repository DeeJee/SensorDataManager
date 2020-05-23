using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;

namespace SensorData.Api.infrastructure
{
    public class ExceptionFilter: Microsoft.AspNetCore.Mvc.Filters.IExceptionFilter
    {
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public void OnException(ExceptionContext context)
        {
            logger.Error(context.Exception);
            context.HttpContext.Response.StatusCode = 500;
            context.Result = new ObjectResult("");
        }
    }
}
