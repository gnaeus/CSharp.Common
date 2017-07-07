using System.Web;
using System.Web.Mvc;
using NLog;

namespace AspNet.Mvc.Common.Logging
{
    /// <summary>
    /// Global exception logger for AspNet MVC.
    /// </summary>
    public class LoggingErrorAttribute : HandleErrorAttribute
    {
        private readonly ILogger Logger;

        public LoggingErrorAttribute(ILogger logger)
        {
            Logger = logger;
        }

        public LoggingErrorAttribute()
            : this(DependencyResolver.Current.GetService<ILogger>()) { }
        
        public override void OnException(ExceptionContext filterContext)
        {
            HttpRequestBase request = filterContext.HttpContext.Request;
            string message = request.HttpMethod + " " + request.Url.PathAndQuery;

            if (filterContext.ExceptionHandled) {
                Logger.Warn(filterContext.Exception, message);
            } else {
                Logger.Error(filterContext.Exception, message);
            }
        }
    }
}
