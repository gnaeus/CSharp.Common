using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using EntityFramework.Common.Entities;
using EntityFramework.Common.Utils;

namespace EntityFramework.Common.Extensions
{
    public static partial class DbContextExtensions
    {
        /// <summary>
        /// Get composite primary key from entity.
        /// </summary>
        public static EntityKeyMember[] GetPrimaryKeys(this DbContext context, object entity)
        {
            return ((IObjectContextAdapter)context)
                .ObjectContext.ObjectStateManager.GetObjectStateEntry(entity)
                .EntityKey.EntityKeyValues;
        }

        /// <summary>
        /// Get changed entities from change tracker.
        /// </summary>
        public static IEnumerable<DbEntityEntry> GetChangedEntries(this DbContext context, EntityState state)
        {
            return context.ChangeTracker.Entries().Where(e => (e.State & state) != 0);
        }

        /// <summary>
        /// Create transaction with `IDbTransaction` interface (instead of `DbContextTransaction`).
        /// </summary>
        public static IDbTransaction BeginTransaction(
            this DbContext context, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new DbContextTransactionWrapper(context.Database.BeginTransaction(isolationLevel));
        }

        /// <summary>
        /// Check if <see cref="DbContext.Database"/> connection is alive.
        /// </summary>
        public static void Touch(this DbContext context)
        {
            context.Database.ExecuteSqlCommand("SELECT 1");
        }

        /// <summary>
        /// Check if <see cref="DbContext.Database"/> connection is alive.
        /// </summary>
        public static Task TouchAsync(this DbContext context)
        {
            return context.Database.ExecuteSqlCommandAsync("SELECT 1");
        }

        /// <summary>
        /// Save changes regardless of <see cref="DbUpdateConcurrencyException"/>.
        /// http://msdn.microsoft.com/en-us/data/jj592904.aspx
        /// </summary>
        public static void SaveChangesIgnoreConcurrency(
            this DbContext context, int retryCount = 3)
        {
            int errorCount = 0;
            for (;;)
            {
                try
                {
                    context.SaveChanges();
                    break;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (++errorCount > retryCount)
                    {
                        throw;
                    }
                    // Update original values from the database 
                    DbEntityEntry entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());

                    UpdateRowVersionFromDb(entry);
                }
            };
        }

        /// <summary>
        /// Save changes regardless of <see cref="DbUpdateConcurrencyException"/>.
        /// http://msdn.microsoft.com/en-us/data/jj592904.aspx
        /// </summary>
        public static async Task SaveChangesIgnoreConcurrencyAsync(
            this DbContext context, int retryCount = 3)
        {
            int errorCount = 0;
            for (;;)
            {
                try
                {
                    await context.SaveChangesAsync();
                    break;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (++errorCount > retryCount)
                    {
                        throw;
                    }
                    // Update original values from the database 
                    DbEntityEntry entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(await entry.GetDatabaseValuesAsync());

                    UpdateRowVersionFromDb(entry);
                }
            };
        }

        private static void UpdateRowVersionFromDb(DbEntityEntry entry)
        {
            var optimisticConcurrent = entry.Entity as IOptimisticConcurrent;
            if (optimisticConcurrent != null)
            {
                optimisticConcurrent.RowVersion = entry.Property("RowVersion").OriginalValue as byte[];
            }
        }
    }
}
