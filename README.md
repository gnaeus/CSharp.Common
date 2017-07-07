
<details id="AspNet.Mvc">
    <summary style="font-size: 1.8em">AspNet.Mvc <a href="./AspNet.Mvc">[docs]</a></summary>

</details>

<details id="AspNet.WebApi">
    <summary style="font-size: 1.8em">AspNet.WebApi <a href="./AspNet.WebApi">[docs]</a></summary>

</details>

<details id="Common.Api">
    <summary style="font-size: 1.8em">Common.Api <a href="./Common/Api">[docs]</a></summary>

## ApiWrapper
Utility for wrapping operation results and logging exceptions.

```cs
public class ApiWrapper : IApiWrapper
{
    public ApiWrapper(NLog.ILogger logger);

    public ApiStatus Execute(Action method);
    public ApiStatus<TError> Execute<TError>(Action method);
    public ApiResult<TResult> Execute<TResult>(Func<TResult> method);
    public ApiResult<TResult, TError> Execute<TResult, TError>(Func<TResult> method);
    public async Task<ApiStatus> ExecuteAsync(Func<Task> method);
    public async Task<ApiStatus<TError>> ExecuteAsync<TError>(Func<Task> method);
    public async Task<ApiResult<TResult>> ExecuteAsync<TResult>(Func<Task<TResult>> method);
    public async Task<ApiResult<TResult, TError>> ExecuteAsync<TResult, TError>(Func<Task<TResult>> method);
}
```

Example:
```cs
using System.Data.SqlClient;
using Common.Api;
using Common.Exceptions;
using static Common.Api.ApiHelper;

class Model { }

enum ErrorCodes { GeneralError }

class WebService
{
    readonly ApiWrapper _apiWrapper;
    readonly ApplicationService _applicationService;

    public ApiResult<Model, ErrorCodes> DoSomething(Model argument)
    {
        return _apiWrapper.Execute<Model, ErrorCodes>(() =>
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

## ApiResult
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

## ApiStatus
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

## ApiHelper
Static helper for wrapping operation results and errors to common structures.

__`Ok()`__  
Utility for returning result from method

__`Ok<TResult>(TResult data)`__  
Utility for returning result from method

__`Error<TError>(TError code, string message = null)`__  
Utility for returning error from method
</details>

<details id="Common.Exceptions">
    <summary style="font-size: 1.8em">Common.Exceptions <a href="./Common/Exceptions">[docs]</a></summary>

## BusinessException
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

## ValidationException
Exception for passing validation errors.

```cs
public class ValidationException : Exception
{
    public ValidationError[] Errors { get; }

    public ValidationException(string path, string code, string message);
    public ValidationException(params ValidationError[] errors);
}
```
</details>

<details id="Common.Extensions">
    <summary style="font-size: 1.8em">Common.Extensions <a href="./Common/Extensions">[docs]</a></summary>

## ConnectionExtensions
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

## MappingExtensions
Extensions for updating `ICollection` of some domain entities from `IEnumerable` of the relevant DTOs

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

static class OrderMapper
{
    public static void UpdateOrder(OrderEntity entity, OrderModel model)
    {
        entity.Id = model.Id;

        entity.Products.UpdateFrom(model.Products)
            .WithKeys(e => e.Id, m => m.Id)
            .MapValues(ProductMapper.UpdateProduct);
    }

    public static OrderModel MapOrder(OrderEntity entity)
    {
        return new OrderModel
        {
            Id = entity.Id,

            Products = entity.Products
                .Select(ProductMapper.MapProduct)
                .ToArray(),
        };
    }
}

static class ProductMapper
{
    public static void UpdateProduct(ProductEntity entity, ProductModel model)
    {
        entity.Id = model.Id;
        entity.Title = model.Title;
    }

    public static ProductModel MapProduct(ProductEntity entity)
    {
        return new ProductModel
        {
            Id = entity.Id,
            Title = entity.Title,
        };
    }
}
```

## ArrayExtensions

__`T[] Add<T>(this T[] array, T item)`__  

__`T[] Remove<T>(this T[] array, T item)`__  

__`T[] Replace<T>(this T[] array, T oldItem, T newItem)`__  

## ByteArrayExtensions

__`bool SequenceEqual(this byte[] first, byte[] second)`__  

__`byte[] ExtractBytes(this byte[] source, int offset, int count)`__  

__`byte[] Concat(this byte[] first, byte[] second)`__  

__`byte[] Combine(params byte[][] arrays)`__  

## EnumerableExtensions

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

## EnumExtensions

__`bool In<TEnum>(this TEnum value, params TEnum[] values)`__  
`color.In(Colors.First, Colors.Second)` is equivalent to `color == Colors.First || color == Colors.Second`.

__`Dictionary<TEnum, bool> ToDictionary<TEnum>(this TEnum value)`__  
Convert `[Flags] enum` to `Dictionary<TEnum, bool>`.

__`Dictionary<TEnum, bool> ToDictionary<TEnum>(this TEnum? value)`__  
Convert nullable `[Flags] enum` to `Dictionary<TEnum, bool>`.

__`TEnum? ToEnum<TEnum>(this IDictionary<TEnum, bool> value)`__  
Convert `Dictionary<TEnum, bool>` to nullable `[Flags] enum`.

## StringExtensions

__`string TrimWhiteSpace(this string input)`__  
Replace all long white space inside string by one space character.

__`bool IsBase64(this string value)`__  
Check if string is Base64 string.

## TaskExtensions

__`T AsSyncronous<T>(this Task<T> task)`__  
Execute `Task` synchronously.

__`void AsSyncronous(this Task task)`__  
Execute `Task` synchronously.
</details>

<details id="Common.Helpers">
    <summary style="font-size: 1.8em">Common.Helpers <a href="./Common/Helpers">[docs]</a></summary>

## StringIntepolationHelper
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
            ) = {filter.Tags.Length}"
        )}
        ORDER BY
        {@switch(filter.SortBy,
            @case(ProductsSortBy.Title, " Title ASC"),
            @case(ProductsSortBy.Price, " Price ASC")
        )}";
    }
}
```

## SqlFullTextSearchHepler
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

        Debug.Assert(ftsQuery ==
            "\"cолнышке*\" NEAR \"лежу*\"\n" +
            " OR FORMSOF(FREETEXT, \"cолнышке\")"+
            " AND FORMSOF(FREETEXT, \"лежу\")");

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

## BitHelper

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

## FileSystemHelper

__`void CleanDirectory(string path)`__  
Reqursively delete all files and folders from directory.

__`string RemoveInvalidCharsFromFileName(string fileName)`__  
Cleanup `fileName` from invalid characters.

## UriHelper

__`string GetHost(string uriString)`__  
"http://localhost/SomeApp" => "localhost"

__`string AddTrailingSlash(string url)`__  
"http://localhost/SomeApp" => "http://localhost/SomeApp/"

__`string ChangeHost(string absoluteUrl, string host)`__  
("http://localhost:8080/SomeApp", "127.0.0.1") => "http://127.0.0.1:8080/SomeApp"

__`bool CanonicalEqual(string url1, string url2)`__  
"http://localhost/SomeApp" == "http://localhost/someapp/"
</details>

<details id="Common.Jobs">
    <summary style="font-size: 1.8em">Common.Jobs <a href="./Common/Jobs">[docs]</a></summary>

## AsyncJobsManager
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
</details>

<details id="Common.Mail">
    <summary style="font-size: 1.8em">Common.Mail <a href="./Common/Mail">[docs]</a></summary>

## MailMessageBinarySerializer
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
</details>

<details id="Common.Utils">
    <summary style="font-size: 1.8em">Common.Utils <a href="./Common/Utils">[docs]</a></summary>

## AsyncLazy
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

## DisposableStream
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

## CryptoRandom
Random class replacement with same API but with usage of RNGCryptoServiceProvider inside.

```cs
public class CryptoRandom : Random { }
```
</details>

<details id="Common.Validation">
    <summary style="font-size: 1.8em">Common.Validation <a href="./Common/Validation">[docs]</a></summary>

</details>

<details id="EntityFramework">
    <summary style="font-size: 1.8em">EntityFramework <a href="./EntityFramework">[docs]</a></summary>

## JsonField
Utility struct for storing complex types as JSON strings in database table.

```cs
struct JsonField<TValue>
    where TValue : class
{
    public string Json { get; set; }
    public TValue Value { get; set; }
}
```

Example:
```cs
using EntityFramework.Common.Utils;

class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Login { get; set; }

    private JsonField<Address> _address;
    // used by EntityFramework
    internal string AddressJson
    {
        get { return _address.Json; }
        set { _address.Json = value; }
    }
    // used by application code
    public Address Address
    {
        get { return _address.Value; }
        set { _address.Value = value; }
    }

    // collection initialization by default
    private JsonField<ICollection<string>> _phones = new HashSet<string>();
    internal string PhonesJson
    {
        get { return _phones.Json; }
        set { _phones.Json = value; }
    }
    public ICollection<string> Phones
    {
        get { return _phones.Value; }
        set { _phones.Value = value; }
    }
}

[NotMapped]
class Address
{
    public string City { get; set; }
    public string Street { get; set; }
    public string Building { get; set; }
}
```

## DbContextExtensions

__`EntityKeyMember[] GetPrimaryKeys(this DbContext context, object entity)`__  
Get composite primary key from entity.

__`IEnumerable<DbEntityEntry> GetChangedEntries(this DbContext context, EntityState state)`__  
Get changed entities from change tracker.

__`static IDbTransaction BeginTransaction(this DbContext context, IsolationLevel isolationLevel)`__  
Create transaction with `IDbTransaction` interface (instead of `DbContextTransaction`).

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

        using (IDbTransaction transaction = _context.BeginTransaction())
        {
            _context.SaveChanges();
        }
    }
}
```

## QueriableExtensions

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

## MappingExtensions
Extensions for updating `ICollection` of some domain entities from `IEnumerable` of the relevant DTOs.

```cs
using EntityFramework.Common.Extensions;

class ProductModel
{
    public int Id { get; set; }
    public string Title { get; set; }
}

class ProductEntity
{
    public int Id { get; set; }
    public string Title { get; set; }
}

class ProductsContext : DbContext
{
    public DbSet<ProductEntity> Products { get; set; }
}

class ProductService
{
    readonly ProductsContext _context;

    public void UpdateProducts(ProductModel[] productModels)
    {
        int[] productIds = productModels.Select(p => p.Id).ToArray();

        List<ProductEntity> productEntities = _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToList();

        _context.Products.UpdateCollection(productEntities, productModels)
            .WithKeys(e => e.Id, m => m.Id)
            .MapValues(UpdateProduct);

        _context.SaveChanges();
    }

    private static void UpdateProduct(ProductEntity entity, ProductModel model)
    {
        entity.Id = model.Id;
        entity.Title = model.Title;
    }
}
```

## Utils.NLogDbInterceptor
`IDbCommandInterceptor` implementation for logging errors from SQL-queries.

```cs
class NLogDbInterceptor : IDbCommandInterceptor 
{
    public NLogDbInterceptor(NLog.ILogger logger);
}
```

## Utils.DbContextTransactionWrapper
A wrapper that allows to present EF transactions (`DbContextTransaction`) as `IDbTransaction`.  
For example, if you need to implement an interface that requires you to return `IDbTransaction`.  
Used by `DbContextExtensions.BeginTransaction()`.

```cs
class DbContextTransactionWrapper : IDbTransaction
{
    public DbContextTransactionWrapper(DbContextTransaction transaction);
}
```
</details>

<details id="MrAdvice">
    <summary style="font-size: 1.8em">MrAdvice <a href="./MrAdvice">[docs]</a></summary>

</details>

<details id="Newtonsoft.Json">
    <summary style="font-size: 1.8em">Newtonsoft.Json <a href="./Newtonsoft.Json">[docs]</a></summary>

## RawJsonConverter
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
</details>

<details id="RazorEngine">
    <summary style="font-size: 1.8em">RazorEngine <a href="./RazorEngine">[docs]</a></summary>

</details>
