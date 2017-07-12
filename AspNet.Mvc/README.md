### HttpCustomErrorResult
`HttpStatusCodeResult` with custom error message in response body.

```cs
public class HttpCustomErrorResult : HttpStatusCodeResult
{
    public HttpCustomErrorResult(HttpStatusCode code, string description);
}
```

### TemporaryRedirectResult
`ActionResult` for (307 Temporary Redirect) HTTP status code.  
Unlike `RedirectResult` (302 Found) keeps Request's HTTP verb.

```cs
public class TemporaryRedirectResult : ActionResult
{
    public string Url { get; }

    public TemporaryRedirectResult(string url);
}
```

Example:
```cs
using System.Web.Mvc;
using System.Net;
using AspNet.Mvc.Common.ActionResults;

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
```

### LoggingErrorAttribute
Global exception logger for AspNet MVC.

```cs
public class LoggingErrorAttribute : HandleErrorAttribute
{
    public LoggingErrorAttribute(NLog.ILogger logger);
}
```

Example:
```cs
using AspNet.Mvc.Common.Logging;

class FilterConfig
{
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
        filters.Add(new LoggingErrorAttribute(NLog.LogManager.GetLogger("*")));
    }
}
```

### TimeZoneHelper
Utility for detecting user's TimeZone.

__`static MvcHtmlString GenerateCookieScrpt()`__  
Inject script tag for populating TimeZone cookie to Razor view.

__`static TimeSpan GetClientTimeZoneOffset(ActionExecutingContext filterContext)`__  
Get TimeZone offset from cookie.

View:
```cs
@using AspNet.Mvc.Common.Helpers

@TimeZoneHelper.GenerateCookieScrpt()
```

Controller:
```cs
using AspNet.Mvc.Common.Helpers;

abstract class ControllerBase : Controller
{
    protected internal TimeSpan TimeZoneOffset { get; private set; }

    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        TimeZoneOffset = TimeZoneHelper.GetClientTimeZoneOffset(filterContext);
    }
}
```

### RedirectRoute
Route that extracts "auth_token" parameter from query string like  
`http://host:port/path?name=value&auth_token=abcdef1234567890`  
and pass "auth_token" and entire URL to action:  
`RedirectResult RedirectController.Redirect(string url, string authToken);`

### EnumConstraint
Custom constraint for AspNet.Mvc Attribute Routing that maps string values in URL to specified enum.  
`[Route("/{enumValue:enum(MyNamespace.MyEnum)}")]`

```cs
using AspNet.Mvc.Common.Routing;

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

enum MyEnum { First, Second }

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


    [Route("/{enumValue:enum(MyEnum)}")]
    public ActionResult GetEnum(MyEnum enumValue)
    {
        throw new NotImplementedException();
    }
}
```
