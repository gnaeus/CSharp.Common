using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace EntityFramework.Extensions
{
    public static class DbContextExtensions
    {
        public static EntityKeyMember[] GetPrimaryKeys(this DbContext context, object entity)
        {
            return ((IObjectContextAdapter)context)
                .ObjectContext.ObjectStateManager.GetObjectStateEntry(entity)
                .EntityKey.EntityKeyValues;
        }

        public static IEnumerable<DbEntityEntry> GetChangedEntries(this DbContext context, EntityState state)
        {
            return context.ChangeTracker.Entries().Where(e => (e.State & state) != 0);
        }
    }
}
