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
    internal string AddressJson
    {
        get { return _address.Json; }
        set { _address.Json = value; }
    }
    public Address Address
    {
        get { return _address.Value; }
        set { _address.Value = value; }
    }

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
