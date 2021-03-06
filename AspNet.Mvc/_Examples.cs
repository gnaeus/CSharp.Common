﻿using System;
using System.Web.Mvc;
using System.Net;
using AspNet.Mvc.Common.ActionResults;
using AspNet.Mvc.Common.Logging;
using AspNet.Mvc.Common.Helpers;
using System.Web.Routing;
using AspNet.Mvc.Common.Routing;
using System.Web.Mvc.Routing;
using System.Threading.Tasks;

partial class _Examples
{
    #region ActionResults

    class HomeController : Controller
    {
        public ActionResult Index()
        {
            return new HttpCustomErrorResult(
                HttpStatusCode.Conflict, "ApplicationSpecificErrorCode");
            // ▶ Response Headers
            // HTTP/1.1 409 Conflict
            // ▶ Response
            // ApplicationSpecificErrorCode
        }

        public ActionResult Home()
        {
            return new TemporaryRedirectResult(Url.Action("Index"));
            // ▶ Response Headers
            // HTTP/1.1 307 Temporary Redirect
            // Location: /Home/Index
        }
    }

    #endregion

    #region Logging

    class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new LoggingErrorAttribute(NLog.LogManager.GetLogger("*")));
        }
    }

    #endregion

    #region Helpers

    abstract class ControllerBase : Controller
    {
        protected internal TimeSpan TimeZoneOffset { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            TimeZoneOffset = TimeZoneHelper.GetClientTimeZoneOffset(filterContext);
        }
    }

    #endregion

    #region Routing

    static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.Add(new RedirectRoute());

            var constraintsResolver = new DefaultInlineConstraintResolver();

            constraintsResolver.ConstraintMap.Add("enum", typeof(EnumConstraint));

            routes.MapMvcAttributeRoutes(constraintsResolver);
        }
    }

    class RedirectController : Controller
    {
        [AllowAnonymous]
        public ActionResult Redirect(string url, string authToken)
        {
            // verify authToken
            if (authToken == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return new TemporaryRedirectResult(url);
        }

        public enum MyEnum { First, Second }

        [Route("/{enumValue:enum(RedirectController.MyEnum)}")]
        public ActionResult GetEnum(MyEnum enumValue)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
