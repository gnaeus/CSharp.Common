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

### MappingExtensions
Extensions for updating `ICollection` of some domain entities from `IEnumerable` of the relevant DTOs.

```cs
List<Entity> entities;
Model[] models;

dbContext.Entities.UpdateCollection(entities, models)
    .WithKeys(e => e.Id, m => m.Id)
    .MapValues((e, m) =>
    {
        e.Property = m.Property;
    });
```

Detailed example:
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
