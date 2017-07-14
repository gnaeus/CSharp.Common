using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EntityFramework.Common.Entities;
using EntityFramework.Common.Extensions;
using Jil;

namespace EntityFramework.Common.Utils
{
    /// <summary>
    /// Utility for capturing transaction logs from <see cref="DbContext.SaveChanges"/>.
    /// Tracked entities must implement <see cref="ITransactionLoggable"/> interface.
    /// </summary>
    public class TransactionLogContext : IDisposable
    {
        readonly DbContext _context;
        readonly Guid _transactionId = Guid.NewGuid();
        readonly DateTime _createdUtc = DateTime.UtcNow;

        readonly List<DbEntityEntry> _insertedEntries = new List<DbEntityEntry>();
        readonly List<DbEntityEntry> _updatedEntries = new List<DbEntityEntry>();
        readonly List<TransactionLog> _deletedLogs = new List<TransactionLog>();
        
        public TransactionLogContext(DbContext context)
        {
            _context = context;

            StoreChangedEntries();
        }
        
        public void Dispose()
        {
            foreach (TransactionLog transactionLog in CreateTransactionLogs())
            {
                _context.Entry(transactionLog).State = EntityState.Added;
            }
        }

        private void StoreChangedEntries()
        {
            foreach (var entry in _context.GetChangedEntries(
                EntityState.Added | EntityState.Modified | EntityState.Deleted))
            {
                if (entry.Entity is ITransactionLoggable)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            _insertedEntries.Add(entry);
                            break;

                        case EntityState.Modified:
                            _updatedEntries.Add(entry);
                            break;

                        case EntityState.Deleted:
                            _deletedLogs.Add(CreateTransactionLog(entry, TransactionLog.DELETE));
                            break;
                    }
                }
            }
        }

        public IEnumerable<TransactionLog> CreateTransactionLogs()
        {
            foreach (DbEntityEntry insertedEntry in _insertedEntries)
            {
                yield return CreateTransactionLog(insertedEntry, TransactionLog.INSERT);
            }
            foreach (DbEntityEntry updateEntry in _updatedEntries)
            {
                yield return CreateTransactionLog(updateEntry, TransactionLog.UPDATE);
            }
            foreach (TransactionLog deletedLog in _deletedLogs)
            {
                yield return deletedLog;
            }
        }

        private TransactionLog CreateTransactionLog(DbEntityEntry entry, string operation)
        {
            object entity = entry.Entity;

            Type entityType = entity.GetType();

            if (_context.Configuration.ProxyCreationEnabled)
            {
                entityType = ObjectContext.GetObjectType(entityType);
            }

            TableAndSchema tableAndSchema = _context.GetTableAndSchemaName(entityType);

            var log = new TransactionLog
            {
                TransactionId = _transactionId,
                CreatedUtc = _createdUtc,
                Operation = operation,
                SchemaName = tableAndSchema.Schema,
                TableName = tableAndSchema.Table,
                EntityType = $"{entityType.FullName}, {entityType.Assembly.GetName().Name}",
            };

            if (operation == TransactionLog.DELETE)
            {
                log.EntityJson = JSON.SerializeDynamic(
                    _context.GetPrimaryKeys(entity).ToDictionary(k => k.Key, k => k.Value));
            }
            else
            {
                log.EntityJson = JSON.SerializeDynamic(
                    entry.CurrentValues.ToObject(), Options.IncludeInherited);
            }

            return log;
        }
    }
}
