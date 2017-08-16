 * [AspNet.Mvc](#link-AspNet.Mvc)
 * [AspNet.WebApi](#link-AspNet.WebApi)
 * [Common.Api](#link-Common.Api)
 * [Common.Exceptions](#link-Common.Exceptions)
 * [Common.Extensions](#link-Common.Extensions)
 * [Common.Helpers](#link-Common.Helpers)
 * [Common.Jobs](#link-Common.Jobs)
 * [Common.Logon](#link-Common.Logon)
 * [Common.Mail](#link-Common.Mail)
 * [Common.MethodMiddleware](#link-Common.MethodMiddleware)
 * [Common.Smtp](#link-Common.Smtp)
 * [Common.Utils](#link-Common.Utils)
 * [Common.Validation](#link-Common.Validation)
 * [EntityFramework](#link-EntityFramework)
 * [MrAdvice](#link-MrAdvice)
 * [Newtonsoft.Json](#link-Newtonsoft.Json)
 * [RazorEngine](#link-RazorEngine)

<hr />

## <a name="link-AspNet.Mvc"></a>[AspNet.Mvc](./AspNet.Mvc)

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

## <a name="link-AspNet.WebApi"></a>[AspNet.WebApi](./AspNet.WebApi)

### FileStreamResult
`IHttpActionResult` for passing streams as files to client (streams are not materialized to RAM).

```cs
public class FileStreamResult : IHttpActionResult
{
    public string FileName { get; }
    public string ContentType { get; }

    public FileStreamResult(Stream stream, string fileName, string contentType = null);
    public FileStreamResult(string filePath, string contentType = null);
}
```

Example:
```cs
class FileController : ApiController
{
    public IHttpActionResult DownloadFile(string filePath)
    {
        return new FileStreamResult(filePath, "application/json");
    }

    public IHttpActionResult DownloadMemoryStream()
    {
        // stream will be disposed later by AspNet Web API
        var ms = new MemoryStream();

        using (StreamWriter writer = new StreamWriter(ms, Encoding.UTF8))
        {
            writer.Write("test test test...");
        }

        ms.Seek(0, SeekOrigin.Begin);

        return new FileStreamResult(ms, "MemoryStream.txt", "text/plain");
    }
}
```

### ApiExceptionLogger
NLog global exception logger for AspNet Web API.

```cs
public class ApiExceptionLogger : ExceptionLogger
{
    public ApiExceptionLogger(NLog.ILogger logger);
}
```

## <a name="link-Common.Api"></a>[Common.Api](./Common/Api)

```cs
using System.Data.SqlClient;
using Common.Api;
using Common.Exceptions;
using Common.MethodMiddleware;
using static Common.Api.ApiHelper;

class Model { }

enum ErrorCodes { GeneralError }

class WebService
{
    readonly MethodDecorator _methodDecorator;
    readonly ApplicationService _applicationService;

    public WebService(ApplicationService applicationService)
    {
        _applicationService = applicationService;

        _methodDecorator = new MethodDecorator()
            .Use(new WrapExceptionMiddleware());
    }

    public ApiResult<Model, ErrorCodes> DoSomething(Model argument)
    {
        return _methodDecorator.Execute(new { argument }, () =>
        {
            return _applicationService.DoSomething(argument);
        });
    }

    public ApiResult<Model, ErrorCodes> DoSomethingElse(Model argument)
    {
        if (argument == null)
        {
            return Error(ErrorCodes.GeneralError, $"Argument {nameof(argument)} is required");
        }
        return Ok(new Model());
    }
}

class ApplicationService
{
    public Model DoSomething(Model argument)
    {
        if (argument == null)
        {
            throw new ValidationException(
                nameof(argument), "Required", $"Argument {nameof(argument)} is required");
        }
        try
        {
            // do something
            return argument;
        }
        catch (SqlException)
        {
            throw new BusinessException<ErrorCodes>(
                ErrorCodes.GeneralError, "Something went wrong, please try again");
        }
    }
}
```

### ApiResult
Structure for passing result of service operation with possible validation and logic errors.

```cs
public class ApiResult<TResult>
{
    public bool IsSuccess { get; set; }
    public virtual TResult Data { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public ValidationError[] ValidationErrors { get; set; }
}

public class ApiResult<TResult, TError>
        where TError : struct
{
    public bool IsSuccess { get; set; }
    public virtual TResult Data { get; set; }
    public TError? ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public ValidationError[] ValidationErrors { get; set; }
}
```

### ApiStatus
Structure for passing status of service operation with possible validation and logic errors.

```cs
public class ApiStatus : IApiStatus, IApiError
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public ValidationError[] ValidationErrors { get; set; }
}

public class ApiStatus<TError> : IApiStatus, IApiError<TError>
    where TError : struct
{
    public bool IsSuccess { get; set; }
    public TError? ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public ValidationError[] ValidationErrors { get; set; };
}
```

### ApiHelper
Static helper for wrapping operation results and errors to common structures.

__`Ok()`__  
Utility for returning result from method

__`Ok<TResult>(TResult data)`__  
Utility for returning result from method

__`Error<TError>(TError code, string message = null)`__  
Utility for returning error from method

## <a name="link-Common.Exceptions"></a>[Common.Exceptions](./Common/Exceptions)

### BusinessException
Exception with error code and message that passed to end user of application.

```cs
public class BusinessException : Exception
{
    public string Code { get; set; }

    public BusinessException(string code, string message);
}

public class BusinessException<TError> : Exception
    where TError : struct
{
    public TError Code { get; set; }

    public BusinessException(TError code, string message);
}
```

### ValidationException
Exception for passing validation errors.

```cs
public class ValidationException : Exception
{
    public ValidationError[] Errors { get; }

    public ValidationException(string path, string code, string message);
    public ValidationException(params ValidationError[] errors);
}
```

## <a name="link-Common.Extensions"></a>[Common.Extensions](./Common/Extensions)

### ConnectionExtensions
Some helpers for `IDbConnection` and `DbConnection`.

__`async Task<IDisposable> EnsureOpenAsync(this DbConnection connection)`__  
If connection is already open before `using()` statement then it stays open after `using()` statement.  
If connection is closed before `using()` statement then it will be opened inside `using()` and closed after `using()`.

__`async Task<Stream> QueryBlobAsStreamAsync(this DbConnection connection, string sql, params object[] parameters)`__  
Read SQL Blob value as stream. Sql Query should return one row with one column.

```cs
using Common.Extensions;

class SqlRepository
{
    readonly DbConnection _connection;
    
    public async Task ExecuteSomeQueryAsync()
    {
        using (await _connection.EnsureOpenAsync())
        {
            // execute some SQL
        }
    }
    
    public async Task<Stream> ReadFileAsync(int fileId)
    {
        Stream stream = await _connection.QueryBlobAsStreamAsync(
            "SELECT Content FROM Files WHERE Id = @fileId",
            new SqlParameter("@fileId", fileId));

        return stream;
    }
}
```

### MappingExtensions
Extensions for updating `ICollection` of some domain entities from `IEnumerable` of the relevant DTOs

```cs
List<Entity> entities;
Model[] models;

entities.MapFrom(entities, models)
    .WithKeys(e => e.Id, m => m.Id)
    .MapElements((e, m) =>
    {
        e.Property = m.Property;
    });
```

Detailed example:
```cs
using System.Collections.Generic;
using System.Linq;

class OrderModel
{
    public int Id { get; set; }
    public ProductModel[] Products { get; set; }
}

class ProductModel
{
    public int Id { get; set; }
    public string Title { get; set; }
}

class OrderEntity
{
    public int Id { get; set; }
    public ICollection<ProductEntity> Products { get; } = new HashSet<ProductEntity>();
}

class ProductEntity
{
    public int Id { get; set; }
    public string Title { get; set; }
}

class OrderService
{
    readonly DbContext _dbContext;
    readonly ProductService _productService

    public static void UpdateOrder(OrderEntity entity, OrderModel model)
    {
        entity.Id = model.Id;

        entity.Products.MapFrom(model.Products)
            .WithKeys(e => e.Id, m => m.Id)
            .OnRemove(_dbContext.Products.Remove)
            .MapElements(_productService.UpdateProduct);
    }
}

class ProductService
{
    public void UpdateProduct(ProductEntity entity, ProductModel model)
    {
        entity.Id = model.Id;
        entity.Title = model.Title;
    }
}
```

### ArrayExtensions

__`T[] Add<T>(this T[] array, T item)`__  

__`T[] Remove<T>(this T[] array, T item)`__  

__`T[] Replace<T>(this T[] array, T oldItem, T newItem)`__  

### ByteArrayExtensions

__`bool SequenceEqual(this byte[] first, byte[] second)`__  

__`byte[] ExtractBytes(this byte[] source, int offset, int count)`__  

__`byte[] Concat(this byte[] first, byte[] second)`__  

__`static byte[] Combine(params byte[][] arrays)`__  
Concat multiple ByteArrays.  

__`byte[] HmacSign(this byte[] rawMessage, byte[] hmacKey)`__  
Sign message with HMAC algorithm. Throws `CryptographicException`.  

__`byte[] HmacExtract(this byte[] hmacMessage, byte[] hmacKey)`__  
Extract HMAC signed message.  

### EnumerableExtensions

__`void ForEach<T>(this IEnumerable<T> source, Action<T> action)`__  
Like `List<T>.ForEach(Action<T> action)`.

__`HashSet<T> ToHashSet<T>(this IEnumerable<T> source)`__  
Create `HashSet<T>` from `IEnumerable<T>`.

__`IEnumerable<TItem> DistinctBy<TItem, TKey>(this IEnumerable<TItem> source, Func<TItem, TKey> keySelector)`__  
Like `Distinct()` but uses values from `keySelector` for equality check.

__`IEnumerable<T> OmitRepeated<T>(this IEnumerable<T> source)`__  
Remove repeated values from sequence.

__`IEnumerable<TItem> OmitRepeatedBy<TItem, TKey>(this IEnumerable<TItem> source, Func<TItem, TKey> keySelector)`__  
Like `OmitRepeated()` but uses values from `keySelector` for equality check.

### EnumExtensions

__`bool In<TEnum>(this TEnum value, params TEnum[] values)`__  
`color.In(Colors.First, Colors.Second)` is equivalent to `color == Colors.First || color == Colors.Second`.

__`Dictionary<TEnum, bool> ToDictionary<TEnum>(this TEnum value)`__  
Convert `[Flags] enum` to `Dictionary<TEnum, bool>`.

__`Dictionary<TEnum, bool> ToDictionary<TEnum>(this TEnum? value)`__  
Convert nullable `[Flags] enum` to `Dictionary<TEnum, bool>`.

__`TEnum? ToEnum<TEnum>(this IDictionary<TEnum, bool> value)`__  
Convert `Dictionary<TEnum, bool>` to nullable `[Flags] enum`.

### StringExtensions

__`string TrimWhiteSpace(this string input)`__  
Replace all long white space inside string by one space character.

__`bool IsBase64(this string value)`__  
Check if string is Base64 string.

### TaskExtensions

__`T AsSyncronous<T>(this Task<T> task)`__  
Execute `Task` synchronously.

__`void AsSyncronous(this Task task)`__  
Execute `Task` synchronously.

## <a name="link-Common.Helpers"></a>[Common.Helpers](./Common/Helpers)

#### StringIntepolationHelper
Simple DSL based on C# 6 String Interpolation for building dynamic SQL queries.

```cs
using static Common.Helpers.StringInterpolationHelper;

class EmployeesFilter
{
    public bool IncludeDepartment { get; set; }
    public int? DepartmentId { get; set; }
    public string[] Names { get; set; }
}

class EmployeesSearchService
{
    public void SearchEmployees(EmployeesFilter filter)
    {
        string sql = $@"
        SELECT
            {@if(filter.IncludeDepartment, @"
                dep.Id,
                dep.Name,"
            )}
            emp.Id,
            emp.Name,
            emp.DepartmentId
        FROM Emloyees AS emp
        {@if(filter.IncludeDepartment, @"
            LEFT JOIN Departments AS dep ON dep.Id = emp.DepartmentId"
        )}
        WHERE
        {@if(filter.DepartmentId != null, @"
            emp.DepartmentId = @DepartmentId",
        @else(@"
            emp.DepartmentId IS NULL"
        ))}
        AND (
            {@foreach(filter.Names, name =>
                $"emp.Name LIKE '{name}%'",
                " OR "
            )}
        )";
    }
}

class ProductsFilter
{
    public string Title { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string[] Tags { get; set; }
    public int? TagsLength => Tags?.Length;
    public ProductsSortBy SortBy { get; set; }
}

enum ProductsSortBy { Title, Price }

class ProductsSearchService
{
    public void SearchProducts(ProductsFilter filter)
    {
        string sql = $@"
        SELECT
            p.Title,
            p.Description,
            p.Price
        FROM Products AS p
        WHERE 1 = 1
        {@if(filter.Title != null, @"
            AND p.Title = @Title"
        )}
        {@if(filter.MinPrice != null, @"
            AND p.Price >= @MinPrice"
        )}
        {@if(filter.MaxPrice != null, @"
            AND p.Price <= @MaxPrice"
        )}
        {@if(filter.Tags != null, $@"
            AND (
                SELECT COUNT(1)
                FROM ProductTags AS t
                WHERE t.ProductId = p.Id
                  AND t.Tag IN (@Tags)
            ) = @TagsLength"
        )}
        ORDER BY
        {@switch(filter.SortBy,
            @case(ProductsSortBy.Title, " Title ASC"),
            @case(ProductsSortBy.Price, " Price ASC")
        )}";
    }
}
```

### SqlFullTextSearchHepler
Utils for Full Text Search in Microsoft SQL Server

__`string PrepareFullTextQuery(string searchPhrase, bool fuzzy = false, int minWordLength = 3)`__  
Build query for SQL Server FTS Engine CONTAINS function.
Result should be passed through ADO.NET SqlParameter due to preventing SQL Injection.  

```cs
using System.Diagnostics;
using Common.Helpers;

class SqlServerFullTextSearchService
{
    public void SearchArticles(string title = "Я на Cолнышке лежу")
    {
        string ftsQuery = SqlFullTextSearchHepler.PrepareFullTextQuery(title, fuzzy: true);

        // "cолнышке*" NEAR "лежу*" OR FORMSOF(FREETEXT, "cолнышке") AND FORMSOF(FREETEXT, "лежу")

        string sql = @"
        SELECT TOP (10)
            a.Id,
            a.Title,
            a.Content,
            fts.[RANK]
        FROM CONTAINSTABLE(Departments, (Title), @ftsQuery) AS fts
        INNER JOIN Articles AS a ON fts.[KEY] = a.ID
        ORDER BY fts.[RANK] DESC";
    }
}
```

### BitHelper

__`ulong MurmurHash3(ulong key)`__  
Compute [MurMurHash](http://zimbry.blogspot.ru/2011/09/better-bit-mixing-improving-on.html)

__`uint ReverseBits(uint value)`__  
Reverse bits in `[Flags] enum` value for use in `OrderBy()` extension

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;

[Flags]
enum UserRoles
{
    Admin = 1, Moderator = 2, User = 4, Reader = 8,
}

class User
{
    public UserRoles Roles { get; set; }
}

static class UserExtensions
{
    public static IEnumerable<User> OrderByRoles(this IEnumerable<User> users)
    {
        return users.OrderByDescending(u => BitHelper.ReverseBits((uint)u.Roles));
    }
}

```

### FileSystemHelper

__`void CleanDirectory(string path)`__  
Reqursively delete all files and folders from directory.

__`string RemoveInvalidCharsFromFileName(string fileName)`__  
Cleanup `fileName` from invalid characters.

### UriHelper

__`string GetHost(string uriString)`__  
"http://localhost/SomeApp" => "localhost"

__`string AddTrailingSlash(string url)`__  
"http://localhost/SomeApp" => "http://localhost/SomeApp/"

__`string ChangeHost(string absoluteUrl, string host)`__  
("http://localhost:8080/SomeApp", "127.0.0.1") => "http://127.0.0.1:8080/SomeApp"

__`bool CanonicalEqual(string url1, string url2)`__  
"http://localhost/SomeApp" == "http://localhost/someapp/"

### TranslitHelper
Utility for performing transliteration.

__`static string TransliterateIcao(char input)`__  
Transliterate `input` symbol. Based on ICAO standard.  

__`static string TransliterateIcao(char input)`__  
Transliterate `input` string symbol by symbol. Based on ICAO standard.

## <a name="link-Common.Jobs"></a>[Common.Jobs](./Common/Jobs)

### AsyncJobsManager
Utility that skips simultaneous execution of async tasks with same type.

__`async Task ExecuteAsync(Func<Task> asyncAction)`__  

__`async Task StopAsync()`__  

```cs
using Common.Jobs;

class JobsService
{
    readonly AsyncJobsManager _asyncJobsManager;
    
    private async Task FirstJob()
    {
        await Task.Delay(200);
    }

    private async Task SecondJob()
    {
        await Task.Delay(300);
    }

    public async Task RunJobs()
    {
        for (int i = 0; i < 5; i++)
        {
            _asyncJobsManager.ExecuteAsync(FirstJob);
            _asyncJobsManager.ExecuteAsync(SecondJob);
        }

        await Task.Delay(500);

        // FirstJob will be executed three times (600ms)
        // SecondJob will be executed two times (600ms)

        await _asyncJobsManager.StopAsync();
    }
}
```

## <a name="link-Common.Logon"></a>[Common.Logon](./Common/Logon)


## <a name="link-Common.Mail"></a>[Common.Mail](./Common/Mail)

### MailMessageBinarySerializer
Utility for de(serialiaing) `MailMessage` to byte array. Supports .NET 4.0, 4.5.

__`static byte[] Serialize(MailMessage msg)`__  

__`static MailMessage Deserialize(byte[] binary)`__  

__`static MailMessage ReadMailMessage(this BinaryReader r)`__  

__`static void Write(this BinaryWriter w, MailMessage msg)`__  

```cs
using Common.Mail;

class DelayedMailSender
{
    public void StoreMessage(string filePath)
    {
        MailMessage msg = new MailMessage(
            new MailAddress("test1@mail.com", "Address1"),
            new MailAddress("test2@mail.com", "Address2"))
        {
            Subject = "subject sucbejct",
            Body = "Message Body",
            IsBodyHtml = false,
            Priority = MailPriority.High,
        };
        msg.CC.Add(new MailAddress("test3@mail.com", "Address3"));
        msg.Bcc.Add(new MailAddress("test4@mail.com"));
        msg.ReplyToList.Add("test5@mail.com");

        byte[] serializedMsg = MailMessageBinarySerializer.Serialize(msg);

        File.WriteAllBytes(filePath, serializedMsg);
    }

    public void SendStoredMessage(string filePath)
    {
        byte[] serializedMsg = File.ReadAllBytes(filePath);

        MailMessage msg = MailMessageBinarySerializer.Deserialize(serializedMsg);

        using (var client = new SmtpClient("mysmtphost")
        {
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
            PickupDirectoryLocation = Path.GetTempPath(),
        })
        {
            client.Send(msg);
        }
    }
}
```

## <a name="link-Common.MethodMiddleware"></a>[Common.MethodMiddleware](./Common/MethodMiddleware)


## <a name="link-Common.Smtp"></a>[Common.Smtp](./Common/Smtp)

### SmtpConnectionSettings

```cs
public class SmtpConnectionSettings
{
    public string Server { get; set; }
    public int Port { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public bool EnableSsl { get; set; }
}
```

### SmtpConnectionChecker
Utility for checking availability of SMTP Server.

```cs
public class SmtpConnectionChecker
{
    public SmtpConnectionChecker(ISmtpConnectionSettings settings);

    public virtual bool ServerIsReady();
}

### SmtpMailSender
Utility for sending mail messages and handling errors.

```cs
public class SmtpMailSender
{
    public SmtpMailSender(SmtpConnectionSettings settings, NLog.ILogger logger);

    public virtual async Task<bool> TrySend(MailMessage message);
    public virtual async Task<bool> TrySend(byte[] serializedMessage);
}
```

Example:
```cs
using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;
using Common.Smtp;

class MailService
{
    readonly SmtpConnectionChecker _checker;
    readonly SmtpMailSender _sender;

    public async Task SendMailMessage(MailMessage message)
    {
        while (!_checker.ServerIsReady())
        {
            await Task.Delay(1000);
        }

        bool success = await _sender.TrySend(message);

        Debug.WriteIf(!success, "MailMessage sending failed");
    }
}
```

## <a name="link-Common.Utils"></a>[Common.Utils](./Common/Utils)

### AsyncLazy
Like `Lazy<T>` but for wrapping async values.

```cs
public class AsyncLazy<T> : Lazy<Task<T>>
{
    public AsyncLazy(Func<T> valueFactory);
    public AsyncLazy(Func<Task<T>> taskFactory);
}
```

Example:
```cs
using Common.Utils;

class AsyncService
{
    readonly AsyncLazy<string> LazyString = new AsyncLazy<string>(async () =>
    {
        await Task.Delay(1000);
        return "lazy string";
    });

    async Task Method()
    {
        string str = await LazyString;
        // do somethig with this str
    }
}
```

### DisposableStream
Wrapper for `Stream` that dispose `boundObject` when stream is disposed.

```cs
public class DisposableStream : Stream
{
    public DisposableStream(IDisposable boundObject, Stream wrappedStream);
}
```

Example:
```cs
class BlobStreamingService
{
    readonly DbConnection _connection;

    async Task<Stream> StreamFile(int fileId)
    {
        // error handling is skipped
        await _connection.OpenAsync();
        DbCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT TOP (1) Content FROM Files WHERE Id = @fileId";
        command.Parameters.Add(new SqlParameter("@fileId", fileId));
        DbDataReader reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();

        // reader will be disposed with wrapped stream
        return new DisposableStream(reader, reader.GetStream(0));
    }
}
```

### CryptoRandom
Random class replacement with same API but with usage of RNGCryptoServiceProvider inside.

```cs
public class CryptoRandom : Random { }
```

## <a name="link-Common.Validation"></a>[Common.Validation](./Common/Validation)


## <a name="link-EntityFramework"></a>[EntityFramework](./EntityFramework)

### DbContextExtensions

__`EntityKeyMember[] GetPrimaryKeys(this DbContext context, object entity)`__  
Get composite primary key from entity.

__`IEnumerable<DbEntityEntry> GetChangedEntries(this DbContext context, EntityState state)`__  
Get changed entities from change tracker.

__`void Touch(this DbContext context)`__  
Check if `DbContext.Database` connection is alive.

__`Task TouchAsync(this DbContext context)`__  
Check if `DbContext.Database` connection is alive.

__`IDbTransaction BeginTransaction(this DbContext context, IsolationLevel isolationLevel)`__  
Create transaction with `IDbTransaction` interface (instead of `DbContextTransaction`).

__`void ExecuteInTransaction(this DbContext context, Action action)`__  
Execute `Action` in existing transaction or create and use new transaction.

__`T ExecuteInTransaction<T>(this DbContext context, Func<T> method)`__  
Execute `Func<T>` in existing transaction or create and use new transaction.

__`Task ExecuteInTransaction(this DbContext context, Func<Task> asyncAction)`__  
Execute `Func<Task>` in existing transaction or create and use new transaction.

__`Task<T> ExecuteInTransaction<T>(this DbContext context, Func<Task<T>> asyncMethod)`__  
Execute `Func<Task<T>>` in existing transaction or create and use new transaction.

__`TableAndSchema GetTableAndSchemaName(this DbContext context, Type entityType)`__  
Get corresponding table name and schema by `entityType`.

__`TableAndSchema[] GetTableAndSchemaNames(this DbContext context, Type entityType)`__  
Get corresponding table name and schema by `entityType`.
Use it if entity is splitted between multiple tables.

```cs
using EntityFramework.Common.Extensions;

class Passport
{
    [Key, Column(Order = 1)]
    public string Series { get; set; }

    [Key, Column(Order = 2)]
    public string Code { get; set; }
}

class PassportsContext : DbContext
{
    public DbSet<Passport> Passports { get; }
}

class PassportsService
{
    readonly PassportsContext _context;

    void Method()
    {
        var passport = new Passport { Series = "123456", Code = "7890" };

        _context.Passports.Add(passport);

        var keys = _context.GetPrimaryKeys(passport);
        // keys == [{ Key = "Series", Value = "123456" }, { Key = "7890", Value = "7890" }]

        var addedEntities = _context.GetChangedEntries(EntityState.Added);
        // addedEntities[0].Entry == passport;

        var tableAndSchema = _context.GetTableAndSchemaName(typeof(Passport));
        // tableAndSchema.TableName == "Passports"; tableAndSchema.Schema == "dbo"

        // uses existing transaction, otherwise creates new one
        _context.ExecuteInTransaction(() =>
        {
            _context.SaveChanges();
            _context.SaveChanges();
        });

        using (IDbTransaction transaction = _context.BeginTransaction())
        {
            _context.SaveChanges();
            transaction.Commit();
        }
    }
}
```

### QueriableExtensions

```cs
async Task<List<TResult>> ExecuteChunkedInQueryAsync<TResult, TParameter>(
    this IQueryable<TResult> baseQuery,
    Expression<Func<TResult, TParameter>> propertyGetter,
    IEnumerable<TParameter> inValues,
    int chunkSize = 500)
```
Converts a query with big IN clause to multiple queries with smaller IN clausesand combines the results.

```cs
using EntityFramework.Common.Extensions;

class Post
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public DateTime Date { get; set; }
}

class BlogContext : DbContext
{
    public DbSet<Post> Posts { get; set; }
}

class PostsService
{
    readonly BlogContext _context;

    public async Task<List<Post>> GetPostsAsync(DateTime sinceDate, IEnumerable<int> authorIds)
    {
        return await _context.Posts
            .Where(p => p.Date >= sinceDate)
            .ExecuteChunkedInQueryAsync(p => p.AuthorId, authorIds, chunkSize: 100);
    }
}
```

### TableAndSchema
Structure that represents table name and schema.

```cs
struct TableAndSchema
{
    public string TableName;
    public string Schema;
}

var (table, schema) = new TableAndSchema();
```

### NLogDbInterceptor
`IDbCommandInterceptor` implementation for logging errors from SQL-queries.

```cs
class NLogDbInterceptor : IDbCommandInterceptor 
{
    public NLogDbInterceptor(NLog.ILogger logger);
}
```

### DbTransactionAdapter
A wrapper that allows to present EF transactions (`DbContextTransaction`) as `IDbTransaction`.  
For example, if you need to implement an interface that requires you to return `IDbTransaction`.  
Used by `DbContextExtensions.BeginTransaction()`.

```cs
class DbTransactionAdapter : IDbTransaction
{
    public DbTransactionAdapter(DbContextTransaction transaction);
}
```

## <a name="link-MrAdvice"></a>[MrAdvice](./MrAdvice)


## <a name="link-Newtonsoft.Json"></a>[Newtonsoft.Json](./Newtonsoft.Json)

### RawJsonConverter
Custom value converter for passing string properties as RAW JSON values.

```cs
using Newtonsoft.Json.Common.Converters;

class Book
{
    [JsonConverter(typeof(RawJsonConverter))]
    public string Chapters { get; set; }
}

class BookService
{
    public string GetBookJson()
    {
        var book = new Book
        {
            Chapters = "[1, 2, 3, 4, 5]",
        };

        return JsonConvert.SerializeObject(book);
        // {"Chapters": [1, 2, 3, 4, 5]}
        // instead of
        // {"Chapters": "[1, 2, 3, 4, 5]"}
    }
}
```

## <a name="link-RazorEngine"></a>[RazorEngine](./RazorEngine)

### MailTemplateEngine
Utility for creating `System.Net.Mail.MailMessage` from Razor view.

```cs
public class MailTemplateEngine
{
    public MailMessage CreateMessage(
        string from,
        string to,
        string templatePath,
        object model,
        string fromName = null,
        bool isBodyHtml = false);

    public Attachment CreateAttachment(
        string templatePath,
        object model,
        string mediaType = "text/plain");
}
```

Example:

```cs
using System.Net.Mail;
using RazorEngine.Common.Mail;

class EmailModel
{
    public string UserName { get; set; }
    public string PasswordLink { get; set; }
}

class EmailService
{
    readonly MailTemplateEngine _mailTemplateEngine;

    public MailMessage CreateEmail(EmailModel model)
    {
        MailMessage message = _mailTemplateEngine.CreateMessage(
            from: "site@example.com",
            to: "user@example.com",
            templatePath: "~/Views/Email/ResetPassword.cshtml",
            model: model,
            fromName: "My awesome site",
            isBodyHtml: true);

        Attachment attachment = _mailTemplateEngine.CreateAttachment(
            templatePath: "~/Views/Email/Attachments/ResetPassword.cshtml",
            model: model);

        attachment.ContentId = "password-links";

        message.Attachments.Add(attachment);

        return message;
    }
}
```

~/Views/Email/ResetPassword.cshtml
```html
@model EmailModel
@{ 
    ViewBag.Subject = "Please reset your password";
}
Dear @Model.UserName, please <a href="@Model.PasswordLink">reset</a> your password.
```

~/Views/Email/Attachments/ResetPassword.cshtml
```html
@model EmailModel
@{
    ViewBag.FileName = "reset_password_link_" + Model.UserName + ".txt";
}
@Model.PasswordLink
```

