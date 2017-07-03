using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Common.Extensions;

partial class _Examples
{
    #region ConnectionExtensions

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

    #endregion

    // TODO
}