using System.Data;
using System.Data.Entity;

namespace EntityFramework.Common.Utils
{
    /// <summary>
    /// A wrapper that allows to present EF transactions (<see cref="DbContextTransaction"/>) as <see cref="IDbTransaction"/>.
    /// For example, if you need to implement an interface that requires you to return <see cref="IDbTransaction"/>.
    /// Used by <see cref="DbContextExtensions.BeginTransaction"/>.
    /// </summary>
    public sealed class DbContextTransactionWrapper : IDbTransaction
    {
        private readonly DbContextTransaction ContextTransaction;

        public DbContextTransactionWrapper(DbContextTransaction transaction)
        {
            ContextTransaction = transaction;
        }

        public void Commit()
        {
            ContextTransaction.Commit();
        }

        public IDbConnection Connection
        {
            get { return ContextTransaction.UnderlyingTransaction.Connection; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return ContextTransaction.UnderlyingTransaction.IsolationLevel; }
        }

        public void Rollback()
        {
            ContextTransaction.Rollback();
        }

        public void Dispose()
        {
            ContextTransaction.Dispose();
        }
    }
}
