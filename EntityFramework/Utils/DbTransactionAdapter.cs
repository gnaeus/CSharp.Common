using System.Data;
using System.Data.Entity;

namespace EntityFramework.Common.Utils
{
    /// <summary>
    /// A wrapper that allows to present EF transactions (<see cref="DbContextTransaction"/>) as <see cref="IDbTransaction"/>.
    /// For example, if you need to implement an interface that requires you to return <see cref="IDbTransaction"/>.
    /// Used by <see cref="DbContextExtensions.BeginTransaction"/>.
    /// </summary>
    public sealed class DbTransactionAdapter : IDbTransaction
    {
        readonly DbContextTransaction _dbContextTransaction;

        public DbTransactionAdapter(DbContextTransaction transaction)
        {
            _dbContextTransaction = transaction;
        }

        public void Commit()
        {
            _dbContextTransaction.Commit();
        }

        public IDbConnection Connection
        {
            get { return _dbContextTransaction.UnderlyingTransaction.Connection; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return _dbContextTransaction.UnderlyingTransaction.IsolationLevel; }
        }

        public void Rollback()
        {
            _dbContextTransaction.Rollback();
        }

        public void Dispose()
        {
            _dbContextTransaction.Dispose();
        }
    }
}
