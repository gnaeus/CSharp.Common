## ConnectionExtensions
Some helpers for `IDbConnection` and `DbConnection`.

__`async Task<IDisposable> EnsureOpenAsync(this DbConnection connection)`__  
If connection is already open before `using()` statement then it stays open after `using()` statement.  
If connection is closed before `using()` statement then it will be opened inside `using()` and closed after `using()`.

__`async Task<Stream> QueryBlobAsStreamAsync(this DbConnection connection, string sql, params object[] parameters)`__  
Read SQL Blob value as stream. Sql Query should return one row with one column.

```cs
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