using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AspNet.Mvc.Common.Routing
{
    /// <summary>
    /// Route that extracts "auth_token" parameter from query string
    /// and pass "auth_token" and entire URL to action: <para />
    /// RedirectResult RedirectController.Redirect(string url, string authToken);
    /// </summary>
    public class RedirectRoute : RouteBase
    {
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            Uri requestUrl = httpContext.Request.Url;
            NameValueCollection requestQuery = httpContext.Request.QueryString;


            string token = requestQuery["auth_token"];

            if (token == null || requestUrl == null)
            {
                return null;
            }

            // HACK: should be an instance of internal HttpValueCollection class
            // see http://stackoverflow.com/questions/829080/how-to-build-a-query-string-for-a-url-in-c
            NameValueCollection resultQuery = HttpUtility.ParseQueryString(requestQuery.ToString());

            resultQuery.Remove("auth_token");

            var routeData = new RouteData(this, new MvcRouteHandler());

            routeData.Values.Add("controller", "Redirect");
            routeData.Values.Add("action", "Redirect");
            routeData.Values.Add("token", token);
            routeData.Values.Add("url", requestUrl.AbsolutePath + "?" + resultQuery);

            return routeData;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return null;
        }
    }
}
