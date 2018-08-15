using NLog;
using System.Web.Http.ExceptionHandling;

namespace SensorDataApi
{
    public class CustomExceptionLogger : ExceptionLogger
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override void Log(ExceptionLoggerContext context)
        {
            logger.Info("CustomExceptionLogger");
            logger.Error(context.ExceptionContext.Exception.ToString());
        }
    }
}