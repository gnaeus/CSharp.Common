using System;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EntityFramework.Common.Helpers
{
    /// <summary>
    /// Utility for retrying SQL-query methods when deadlock is detected.
    /// </summary>
    public static class EfDeadlockHelper
    {
        public const byte APP_DEADLOCK_STATE = 10;

        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="EntityCommandExecutionException" />
        /// <exception cref="SqlException" />
        public static async Task RetryAsync(
            Func<Task> sqlCommandMethod, int maxRetries = 3, int delayMilliseconds = 100)
        {
            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetries));
            }
            if (delayMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delayMilliseconds));
            }

            int tryCount = 0;

            for (;;)
            {
                try
                {
                    await sqlCommandMethod();
                    return;
                }
                catch (EntityCommandExecutionException ex)
                {
                    SqlException sqlEx = ex.InnerException as SqlException;

                    if (sqlEx == null) { throw; }

                    if (sqlEx.Number != 1205) { throw; }
                    
                    if (++tryCount == maxRetries) { throw; }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 50000 && ex.State == APP_DEADLOCK_STATE)
                    {
                        if (++tryCount == maxRetries) { throw; }
                    }
                    else
                    {
                        throw;
                    }
                }
                await Task.Delay(delayMilliseconds);
            }
        }

        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="EntityCommandExecutionException" />
        /// <exception cref="SqlException" />
        public static async Task<T> RetryAsync<T>(
            Func<Task<T>> sqlQueryMethod, int maxRetries = 3, int delayMilliseconds = 100)
        {
            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetries));
            }
            if (delayMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delayMilliseconds));
            }

            int tryCount = 0;

            for (;;)
            {
                try
                {
                    return await sqlQueryMethod();
                }
                catch (EntityCommandExecutionException ex)
                {
                    SqlException sqlEx = ex.InnerException as SqlException;

                    if (sqlEx == null) { throw; }

                    if (sqlEx.Number != 1205) { throw; }

                    if (++tryCount == maxRetries) { throw; }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 50000 && ex.State == APP_DEADLOCK_STATE)
                    {
                        if (++tryCount == maxRetries) { throw; }
                    }
                    else
                    {
                        throw;
                    }
                }
                await Task.Delay(delayMilliseconds);
            }
        }
    }
}
