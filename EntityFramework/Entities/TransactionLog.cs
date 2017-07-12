using System;
using Jil;

namespace EntityFramework.Common.Entities
{
    /// <summary>
    /// This interface is implemented by entities which wanted
    /// to store all modifications in <see cref="TransactionLog"/>.
    /// </summary>
    public interface ITransactionLoggable { }

    public class TransactionLog
    {
        public const char INSERT = 'I';
        public const char UPDATE = 'U';
        public const char DELETE = 'D';

        public long Id { get; set; }
        public Guid TransactionId { get; set; }
        public DateTime CreatedUtc { get; set; }
        public char Operation { get; set; }
        public string TableName { get; set; }
        public string EntityType { get; set; }
        public string EntityJson { get; set; }

        private Lazy<object> _entity;
        public object Entity => _entity.Value;

        public TEntity GetEntity<TEntity>()
            where TEntity : class
        {
            return _entity.Value as TEntity;
        }

        public TransactionLog()
        {
            _entity = new Lazy<object>(() =>
            {
                return JSON.Deserialize(EntityJson, Type.GetType(EntityType));
            });
        }
    }
}
