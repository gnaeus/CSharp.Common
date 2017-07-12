using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using EntityFramework.Common.Utils;
using EntityFramework.Common.Entities;

namespace EntityFramework.Common.Extensions
{
    public static partial class DbContextExtensions
    {
        /// <summary>
        /// Wrapper for <see cref="DbContext.SaveChanges"/> that saves <see cref="TransactionLog"/> to DB.
        /// </summary>
        public static int SaveChangesWithTransactionLog(
            this DbContext dbContext, Func<int> saveChanges)
        {
            int count;

            // TODO: check transaction
            // using (DbContextTransaction transaction = dbContext.WithTransaction())
            using (TransactionLogContext logContext = new TransactionLogContext(dbContext))
            {
                // save main entities
                count = saveChanges.Invoke();

                logContext.StoreTransactionLogs();

                // save TransactionLog entries
                saveChanges.Invoke();

                // transaction.Commit();
            }

            return count;
        }

        /// <summary>
        /// Wrapper for <see cref="DbContext.SaveChangesAsync"/> that saves <see cref="TransactionLog"/> to DB.
        /// </summary>
        public static async Task<int> SaveChangesWithTransactionLogAsync(
            this DbContext dbContext,
            Func<Task<int>> saveChangesAsync,
            CancellationToken cancellationToken)
        {
            int count;

            using (DbContextTransaction transaction = dbContext.WithTransaction())
            using (TransactionLogContext logContext = new TransactionLogContext(dbContext))
            {
                // save main entities
                count = await saveChangesAsync.Invoke();

                logContext.StoreTransactionLogs();

                // save TransactionLog entries
                await saveChangesAsync.Invoke();

                transaction.Commit();
            }
            return count;
        }
    }

    public static partial class ModelBuilderExtensions
    {
        /// <summary>
        /// Register <see cref="TransactionLog"/> table in <see cref="DbContext"/>.
        /// </summary>
        public static DbModelBuilder EnableTransactionLog(
            this DbModelBuilder modelBuilder,
            string tableName = "_TransactionLog",
            string schemaName = null)
        {
            var transactionLog = modelBuilder.Entity<TransactionLog>();
             
            transactionLog.HasKey(e => e.Id);

            transactionLog.Property(e => e.TableName).IsRequired();
            transactionLog.Property(e => e.EntityType).IsRequired();
            transactionLog.Property(e => e.EntityJson).IsRequired();

            if (schemaName == null)
            {
                transactionLog.ToTable(tableName);
            }
            else
            {
                transactionLog.ToTable(tableName, schemaName);
            }

            return modelBuilder;
        }
    }
}
