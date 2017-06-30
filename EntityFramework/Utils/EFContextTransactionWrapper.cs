using System.Data;
using System.Data.Entity;

namespace EntityFramework.Utils
{  
    /// <summary>
    /// A wrapper that allows to present EF transactions (DbContextTransaction) as IDbTransactions
    /// For example, if you need to implement an interface that requires you to return IDbTransaction
    /// </summary>
    public sealed class EFContextTransactionWrapper : IDbTransaction
    {
        private readonly DbContextTransaction ContextTransaction;

        public EFContextTransactionWrapper(DbContextTransaction transaction)
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
