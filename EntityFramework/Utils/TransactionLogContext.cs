using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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

        readonly List<DbEntityEntry> _added = new List<DbEntityEntry>();
        readonly List<DbEntityEntry> _modified = new List<DbEntityEntry>();
        readonly List<DbEntityEntry> _deleted = new List<DbEntityEntry>();
        
        const EntityState CHANGED = EntityState.Added | EntityState.Modified | EntityState.Deleted;

        public TransactionLogContext(DbContext context)
        {
            _context = context;

            foreach (var entry in context.GetChangedEntries(CHANGED))
            {
                if (entry.Entity is ITransactionLoggable)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            _added.Add(entry);
                            break;

                        case EntityState.Modified:
                            _modified.Add(entry);
                            break;

                        case EntityState.Deleted:
                            _deleted.Add(entry);
                            break;
                    }
                }
            }
        }

        private bool _disposed = false;

        public void Dispose()
        {
            _disposed = true;
        }

        public void StoreTransactionLogs()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionLogContext));
            }

            foreach (TransactionLog transactionLog in GetTransactionLogs())
            {
                _context.Entry(transactionLog).State = EntityState.Added;
            }
        }

        public IEnumerable<TransactionLog> GetTransactionLogs()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionLogContext));
            }

            foreach (DbEntityEntry entry in _added)
            {
                yield return MakeTransactionLog(entry, TransactionLog.INSERT);
            }
            foreach (DbEntityEntry entry in _modified)
            {
                yield return MakeTransactionLog(entry, TransactionLog.UPDATE);
            }
            foreach (DbEntityEntry entry in _deleted)
            {
                yield return MakeTransactionLog(entry, TransactionLog.DELETE);
            }
        }

        private TransactionLog MakeTransactionLog(DbEntityEntry entry, char operation)
        {
            object entity = entry.Entity;

            Type entityType = entity.GetType();

            return new TransactionLog
            {
                TransactionId = _transactionId,
                CreatedUtc = _createdUtc,
                Operation = operation,
                TableName = _context.GetTableName(entityType),
                EntityType = $"{entityType.FullName}, {entityType.Assembly.FullName}",
                EntityJson = JSON.SerializeDynamic(entry.CurrentValues.ToObject()),
            };
        }
    }
}
