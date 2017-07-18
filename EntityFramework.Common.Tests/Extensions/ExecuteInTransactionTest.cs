using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Threading.Tasks;
using EntityFramework.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Common.Tests.Extensions
{
    [TestClass]
    public class ExecuteInTransactionTest
    {
        private DbConnection _connection;

        [TestInitialize]
        public void TestInitialize()
        {
            _connection = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");

            _connection.Open();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _connection.Close();
        }

        public class TestDbContext : DbContext
        {
            public TestDbContext(DbConnection connection)
                : base(connection, false)
            {
                Database.Log = s => Debug.WriteLine(s);
                Database.SetInitializer<TestDbContext>(null);
            }
        }

        [TestMethod]
        public void ShouldCreateNewTransaction()
        {
            using (var context = new TestDbContext(_connection))
            {
                Assert.IsNull(context.Database.CurrentTransaction);

                int methodCalls = 0;

                context.ExecuteInTransaction(() =>
                {
                    methodCalls++;

                    Assert.IsNotNull(context.Database.CurrentTransaction);

                    return 0;
                });

                Assert.IsNull(context.Database.CurrentTransaction);
                Assert.AreEqual(1, methodCalls);
            }
        }

        [TestMethod]
        public async Task ShouldCreateNewTransactionAsync()
        {
            using (var context = new TestDbContext(_connection))
            {
                Assert.IsNull(context.Database.CurrentTransaction);

                int methodCalls = 0;

                await context.ExecuteInTransaction(async () =>
                {
                    methodCalls++;

                    Assert.IsNotNull(context.Database.CurrentTransaction);

                    await Task.Delay(1);

                    return 0;
                });

                Assert.IsNull(context.Database.CurrentTransaction);
                Assert.AreEqual(1, methodCalls);
            }
        }

        [TestMethod]
        public void ShouldPreserveExistingTransaction()
        {
            using (var context = new TestDbContext(_connection))
            using (var transaction = context.Database.BeginTransaction())
            {
                int methodCalls = 0;

                context.ExecuteInTransaction(() =>
                {
                    methodCalls++;

                    Assert.AreEqual(transaction, context.Database.CurrentTransaction);

                    return 0;
                });

                Assert.AreEqual(transaction, context.Database.CurrentTransaction);
                Assert.AreEqual(1, methodCalls);
            }
        }

        [TestMethod]
        public async Task ShouldPreserveExistingTransactionAsync()
        {
            using (var context = new TestDbContext(_connection))
            using (var transaction = context.Database.BeginTransaction())
            {
                int methodCalls = 0;

                await context.ExecuteInTransaction(async () =>
                {
                    methodCalls++;

                    Assert.AreEqual(transaction, context.Database.CurrentTransaction);

                    await Task.Delay(1);

                    return 0;
                });

                Assert.AreEqual(transaction, context.Database.CurrentTransaction);
                Assert.AreEqual(1, methodCalls);
            }
        }
    }
}
