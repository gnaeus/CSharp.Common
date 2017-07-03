using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EntityFramework.Common.Utils;

namespace EntityFramework.Common.Extensions
{
    public static class DbContextExtensions
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
    }
}
