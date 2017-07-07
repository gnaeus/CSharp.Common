using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AspNet.Mvc.Common.ActionResults
{
    /// <summary>
    /// <see cref="HttpStatusCodeResult"/> with custom error message in response body.
    /// </summary>
    public class HttpCustomErrorResult : HttpStatusCodeResult
    {
        private readonly string _description;
        
        public HttpCustomErrorResult(HttpStatusCode code, string description)
            : base(code)
        {
            _description = description;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            HttpResponseBase response = context.HttpContext.Response;

            response.TrySkipIisCustomErrors = true;

            response.Clear();

            if (_description != null)
            {
                response.Write(_description);
            }

            base.ExecuteResult(context);
        }
    }
}
