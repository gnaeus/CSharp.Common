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
