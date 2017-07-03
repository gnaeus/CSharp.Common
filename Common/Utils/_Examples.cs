using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Common.Utils;

partial class _Examples
{
    #region AsyncLazy

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

    #endregion

    #region DisposableStream

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

    #endregion
}
