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
