using System;
using System.Web;
using System.Web.Mvc;

namespace AspNet.Mvc.Common.Helpers
{
    public static class TimeZoneHelper
    {
        private const string CookieName = "tzOffset";

        public static TimeSpan GetClientTimeZoneOffset(ActionExecutingContext filterContext)
        {
            HttpContextBase context = filterContext.HttpContext;
            HttpCookie cookie = context.Request.Cookies[CookieName];
            
            long offsetMinutes;
            if (cookie != null && Int64.TryParse(cookie.Value, out offsetMinutes)) {
                // in JS time zone offset is negative
                // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date/getTimezoneOffset
                return TimeSpan.FromMinutes(-offsetMinutes);
            }
            return default (TimeSpan);
        }

        public static MvcHtmlString GenerateCookieScrpt()
        {
            string script = String.Format(
                "<script type=\"text/javascript\">" +
                    "document.cookie = '{0}=' + new Date().getTimezoneOffset() + ';path={1}';" +
                "</script>", CookieName, HttpRuntime.AppDomainAppVirtualPath);

            return new MvcHtmlString(script);
        }
    }
}
