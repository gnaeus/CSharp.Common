using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.Utils;

namespace Common.Extensions
{
    public static class ConnectionExtensions
    {
        private struct ConnectionToken : IDisposable
        {
            private readonly IDbConnection _connection;

            public ConnectionToken(IDbConnection connection)
            {
                _connection = connection;
            }

            public void Dispose()
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// If connection is already open before `using()` statement
        /// then it stays open after `using()` statement
        /// <para/>
        /// If connection is closed before `using()` statement
        /// then it will be opened inside `using()` and closed after `using()`
        /// </summary>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        public static IDisposable EnsureOpen(this IDbConnection connection)
        {
            if (connection == null) {
                throw new ArgumentNullException("connection");
            }
            switch (connection.State) {
                case ConnectionState.Open:
                    return null;
                case ConnectionState.Broken:
                    connection.Close();
                    break;
                case ConnectionState.Closed:
                    break;
                default:
                    throw new InvalidOperationException("Connection already in use");
            }
            connection.Open();
            return new ConnectionToken(connection);
        }

        /// <summary>
        /// If connection is already open before `using()` statement
        /// then it stays open after `using()` statement
        /// <para/>
        /// If connection is closed before `using()` statement
        /// then it will be opened inside `using()` and closed after `using()`
        /// </summary>
        public static Task<IDisposable> EnsureOpenAsync(this DbConnection connection)
        {
            return EnsureOpenAsync(connection, CancellationToken.None);
        }

        /// <summary>
        /// If connection is already open before `using()` statement
        /// then it stays open after `using()` statement
        /// <para/>
        /// If connection is closed before `using()` statement
        /// then it will be opened inside `using()` and closed after `using()`
        /// </summary>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        public static async Task<IDisposable> EnsureOpenAsync(
            this DbConnection connection, CancellationToken cancellationToken)
        {
            if (connection == null) {
                throw new ArgumentNullException("connection");
            }
            switch (connection.State) {
                case ConnectionState.Open:
                    return null;
                case ConnectionState.Broken:
                    connection.Close();
                    break;
                case ConnectionState.Closed:
                    break;
                default:
                    throw new InvalidOperationException("Connection already in use");
            }
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return new ConnectionToken(connection);
        }

        /// <summary>
        /// Read SQL Blob value as stream. Sql Query should return one row with one column.<para />
        /// Example: Stream stream = await Conn.QueryBlobAsStreamAsync(
        ///     "SELECT Content FROM Files WHERE Id = @fileId",
        ///     new SqlParameter("@fileId", fileId));
        /// </summary>
        public static async Task<Stream> QueryBlobAsStreamAsync(
            this DbConnection connection, string sql, params object[] parameters)
        {
            DbDataReader reader = null;

            bool disposing = true;
            try {
                await connection.OpenAsync();

                DbCommand command = connection.CreateCommand();

                command.CommandText = sql;

                command.Parameters.AddRange(parameters);

                reader = await command.ExecuteReaderAsync(
                    CommandBehavior.SequentialAccess |
                    CommandBehavior.SingleResult |
                    CommandBehavior.SingleRow |
                    CommandBehavior.CloseConnection
                );

                if (await reader.ReadAsync()) {
                    disposing = false;
                    return new DisposableStream(reader, reader.GetStream(0));
                } else {
                    return null;
                }
            }
            finally {
                if (disposing) {
                    reader?.Dispose();
                    connection?.Dispose();
                }
            }
        }
    }
}
