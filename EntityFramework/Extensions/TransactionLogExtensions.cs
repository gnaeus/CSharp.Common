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
            return dbContext.ExecuteInTransaction(() =>
            {
                int count;
                using (new TransactionLogContext(dbContext))
                {
                    // save main entities
                    count = saveChanges.Invoke();
                }

                // save TransactionLog entities
                saveChanges.Invoke();

                return count;
            });
        }

        /// <summary>
        /// Wrapper for <see cref="DbContext.SaveChangesAsync"/> that saves <see cref="TransactionLog"/> to DB.
        /// </summary>
        public static Task<int> SaveChangesWithTransactionLogAsync(
            this DbContext dbContext,
            Func<CancellationToken, Task<int>> saveChangesAsync,
            CancellationToken cancellationToken)
        {
            return dbContext.ExecuteInTransaction(async () =>
            {
                int count;
                using (new TransactionLogContext(dbContext))
                {
                    // save main entities
                    count = await saveChangesAsync.Invoke(cancellationToken);
                }

                // save TransactionLog entities
                await saveChangesAsync.Invoke(cancellationToken);

                return count;
            });
        }
    }

    public static partial class ModelBuilderExtensions
    {
        /// <summary>
        /// Register <see cref="TransactionLog"/> table in <see cref="DbContext"/>.
        /// </summary>
        public static DbModelBuilder UseTransactionLog(
            this DbModelBuilder modelBuilder,
            string tableName = "_TransactionLog",
            string schemaName = null)
        {
            var transactionLog = modelBuilder.Entity<TransactionLog>();

            if (schemaName == null)
            {
                transactionLog.ToTable(tableName);
            }
            else
            {
                transactionLog.ToTable(tableName, schemaName);
            }

            transactionLog
                .HasKey(e => e.Id);

            transactionLog
                .Property(e => e.Operation)
                .IsRequired()
                .HasMaxLength(3);

            transactionLog
                .Property(e => e.TableName)
                .IsRequired();

            transactionLog
                .Property(e => e.EntityType)
                .IsRequired();

            transactionLog
                .Property(e => e.EntityJson)
                .IsRequired();

            return modelBuilder;
        }
    }
}
