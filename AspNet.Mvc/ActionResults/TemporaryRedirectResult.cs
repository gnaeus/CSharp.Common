using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AspNet.Mvc.Common.ActionResults
{
    /// <summary>
    /// <see cref="ActionResult"/> for (307 Temporary Redirect) HTTP status code.
    /// Unlike <see cref="RedirectResult"/> (302 Found) keeps Request's HTTP verb.
    /// </summary>
    public class TemporaryRedirectResult : ActionResult
    {
        public string Url { get; }

        public TemporaryRedirectResult(string url)
        {
            Url = url;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null) {
                throw new ArgumentNullException("context");
            }
            if (context.IsChildAction) {
                throw new InvalidOperationException("Cannot redirect in child action");
            }

            context.Controller.TempData.Keep();

            HttpResponseBase response = context.HttpContext.Response;

            response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
            response.RedirectLocation = UrlHelper.GenerateContentUrl(Url, context.HttpContext);
        }
    }
}
