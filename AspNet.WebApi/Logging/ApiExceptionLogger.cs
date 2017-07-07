using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using NLog;

namespace AspNet.WebApi.Common.Logging
{
    /// <summary>
    /// NLog global exception logger for AspNet Web API.
    /// </summary>
    public class ApiExceptionLogger : ExceptionLogger
    {
        private readonly ILogger Logger;

        public ApiExceptionLogger(ILogger logger)
        {
            Logger = logger;
        }

        public override void Log(ExceptionLoggerContext context)
        {
            HttpRequestMessage request = context.Request;
            string message = request.Method + " " + request.RequestUri.PathAndQuery;

            Logger.Error(context.Exception, message);
        }
    }
}
