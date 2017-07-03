using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Globalization;
using System.Linq;
using System.Text;
using NLog;

namespace EntityFramework.Common.Utils
{
    /// <summary>
    /// Interceptor for logging errors from SQL-queries.
    /// </summary>
    public class NLogDbInterceptor : IDbCommandInterceptor 
    {
        protected readonly ILogger Logger;

        public NLogDbInterceptor(ILogger logger)
        {
            Logger = logger;
        }
        
        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        protected virtual void LogIfError<TResult>(
            DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
            if (interceptionContext.Exception != null) {
                string connectionString = String.Join(Environment.NewLine,
                    interceptionContext.DbContexts.Select(c => c.Database.Connection.ConnectionString));

                Logger.Log(new LogEventInfo {
                    TimeStamp = DateTime.UtcNow,
                    Level = LogLevel.Error,
                    LoggerName = Logger.Name,
                    FormatProvider = CultureInfo.InvariantCulture,
                    Properties = {
                        { "Database", connectionString },
                        { "Command", command.CommandText },
                        { "Parameters", GetParametersText(command.Parameters) },
                        { "Exception", GetExceptionText(interceptionContext.Exception) }
                    }
                });      
            }
        }

        protected virtual string GetParametersText(DbParameterCollection parameters)
        {
            var sb = new StringBuilder();
            foreach (DbParameter p in parameters) {
                sb.AppendLine(p.ParameterName + " = " + (p.Value ?? ""));
            }
            return sb.ToString();
        }

        protected virtual string GetExceptionText(Exception exception)
        {
            var sb = new StringBuilder();

            // first available stack trace
            string stackTrace = null;
            while (exception != null)
            {
                sb.AppendLine(exception.GetType().FullName + ": " + exception.Message);
                if (String.IsNullOrEmpty(stackTrace))
                {
                    stackTrace = exception.StackTrace;
                }
                exception = exception.InnerException;
            }
            if (stackTrace != null)
            {
                sb.AppendLine(stackTrace);
            }
            return sb.ToString();
        }
    }
}
