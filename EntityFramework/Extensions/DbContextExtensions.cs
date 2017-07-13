using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Mapping;
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

        #region Transactions

        /// <summary>
        /// Create transaction with `IDbTransaction` interface (instead of `DbContextTransaction`).
        /// </summary>
        public static IDbTransaction BeginTransaction(
            this DbContext context, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new DbContextTransactionWrapper(context.Database.BeginTransaction(isolationLevel));
        }

        /// <summary>
        /// Execute <paramref name="action"/> in existing transaction or create and use new transaction.
        /// </summary>
        public static void ExecuteInTransaction(this DbContext context, Action action)
        {
            var currentTransaction = context.Database.CurrentTransaction;
            var transaction = currentTransaction ?? context.Database.BeginTransaction();

            try
            {
                action.Invoke();
                if (transaction != currentTransaction)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (transaction != currentTransaction)
                {
                    transaction.Rollback();
                }
                throw;
            }
            finally
            {
                if (transaction != currentTransaction)
                {
                    transaction.Dispose();
                }
            }
        }

        /// <summary>
        /// Execute <paramref name="method"/> in existing transaction or create and use new transaction.
        /// </summary>
        public static T ExecuteInTransaction<T>(this DbContext context, Func<T> method)
        {
            var currentTransaction = context.Database.CurrentTransaction;
            var transaction = currentTransaction ?? context.Database.BeginTransaction();

            try
            {
                T result = method.Invoke();
                if (transaction != currentTransaction)
                {
                    transaction.Commit();
                }
                return result;
            }
            catch
            {
                if (transaction != currentTransaction)
                {
                    transaction.Rollback();
                }
                throw;
            }
            finally
            {
                if (transaction != currentTransaction)
                {
                    transaction.Dispose();
                }
            }
        }

        /// <summary>
        /// Execute <paramref name="asyncAction"/> in existing transaction or create and use new transaction.
        /// </summary>
        public static async Task ExecuteInTransaction(this DbContext context, Func<Task> asyncAction)
        {
            var currentTransaction = context.Database.CurrentTransaction;
            var transaction = currentTransaction ?? context.Database.BeginTransaction();

            try
            {
                await asyncAction.Invoke();
                if (transaction != currentTransaction)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (transaction != currentTransaction)
                {
                    transaction.Rollback();
                }
                throw;
            }
            finally
            {
                if (transaction != currentTransaction)
                {
                    transaction.Dispose();
                }
            }
        }

        /// <summary>
        /// Execute <paramref name="asyncMethod"/> in existing transaction or create and use new transaction.
        /// </summary>
        public static async Task<T> ExecuteInTransaction<T>(this DbContext context, Func<Task<T>> asyncMethod)
        {
            var currentTransaction = context.Database.CurrentTransaction;
            var transaction = currentTransaction ?? context.Database.BeginTransaction();

            try
            {
                T result = await asyncMethod.Invoke();
                if (transaction != currentTransaction)
                {
                    transaction.Commit();
                }
                return result;
            }
            catch
            {
                if (transaction != currentTransaction)
                {
                    transaction.Rollback();
                }
                throw;
            }
            finally
            {
                if (transaction != currentTransaction)
                {
                    transaction.Dispose();
                }
            }
        }

        #endregion

        #region SaveChangesIgnoreConcurrency

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

        #endregion

        #region GetTableName

        public static string GetTableName(this DbContext context, Type entityType)
        {
            return context.GetTableNames(entityType).Single();
        }

        public static string[] GetTableNames(this DbContext context, Type entityType)
        {
            return _tableNames.GetOrAdd(new ContextEntityType(context.GetType(), entityType), _ =>
            {
                return GetTableNames(entityType, context).ToArray();
            });
        }
        
        private struct ContextEntityType
        {
            public Type ContextType;
            public Type EntityType;

            public ContextEntityType(Type contextType, Type entityType)
            {
                ContextType = contextType;
                EntityType = entityType;
            }

            public override int GetHashCode()
            {
                return ContextType.GetHashCode() ^ EntityType.GetHashCode();
            }
        }

        private static readonly ConcurrentDictionary<ContextEntityType, string[]> _tableNames
            = new ConcurrentDictionary<ContextEntityType, string[]>();

        /// <summary>
        /// https://romiller.com/2014/04/08/ef6-1-mapping-between-types-tables/
        /// </summary>
        private static IEnumerable<string> GetTableNames(Type type, DbContext context)
        {
            var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                .GetItems<EntityType>(DataSpace.OSpace)
                .Single(e => objectItemCollection.GetClrType(e) == type);

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

            // Find the storage entity sets (tables) that the entity is mapped
            var tables = mapping
                .EntityTypeMappings.Single()
                .Fragments;

            // Return the table name from the storage entity set
            return tables.Select(f => (string)f.StoreEntitySet.MetadataProperties["Table"].Value ?? f.StoreEntitySet.Name);
        }

        #endregion
    }
}
