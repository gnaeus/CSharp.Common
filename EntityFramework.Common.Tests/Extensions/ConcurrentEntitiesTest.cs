using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using EntityFramework.Common.Entities;
using EntityFramework.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Common.Tests.Extensions
{
    [TestClass]
    public class ConcurrentEntitiesTest
    {
        private DbConnection _connection;

        [TestInitialize]
        public void TestInitialize()
        {
            _connection = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");

            _connection.Open();

            _connection.Execute(@"
                CREATE TABLE Accounts (
                    Id INTEGER PRIMARY KEY,
                    Login TEXT,
                    RowVersion INTEGER DEFAULT 0
                );

                CREATE TRIGGER TRG_Accounts_UPD
                    AFTER UPDATE ON Accounts
                    WHEN old.RowVersion = new.RowVersion
                BEGIN
                    UPDATE Accounts
                    SET RowVersion = RowVersion + 1;
                END;");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _connection.Close();
        }

        public class Account : IConcurrencyCheckable
        {
            public int Id { get; set; }
            public string Login { get; set; }

            [ConcurrencyCheck]
            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public long RowVersion { get; set; }
        }
        
        public class TestDbContext : DbContext
        {
            public DbSet<Account> Accounts { get; set; }

            public TestDbContext(DbConnection connection)
                : base(connection, false)
            {
                Database.Log = s => Debug.WriteLine(s);
                Database.SetInitializer<TestDbContext>(null);
            }

            public override int SaveChanges()
            {
                this.UpdateConcurrentEntities();
                return base.SaveChanges();
            }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
            {
                this.UpdateConcurrentEntities();
                return base.SaveChangesAsync(cancellationToken);
            }
        }

        [TestMethod, ExpectedException(typeof(DbUpdateConcurrencyException))]
        public void TestConcurrencyCheckableEntities()
        {
            using (var context = new TestDbContext(_connection))
            {
                // insert
                var account = new Account { Login = "first" };
                context.Accounts.Add(account);

                context.SaveChanges();

                Assert.IsNotNull(account.RowVersion);

                // update
                long oldRowVersion = account.RowVersion;

                account.Login = "second";

                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    Assert.Fail();
                }

                Assert.AreNotEqual(oldRowVersion, account.RowVersion);

                // concurrency error
                account.Login = "third";
                account.RowVersion = oldRowVersion;

                context.SaveChanges();
            }
        }


        [TestMethod]
        public void TestSaveChangesIgnoreConcurrency()
        {
            using (var context = new TestDbContext(_connection))
            {
                // insert
                var account = new Account { Login = "first" };
                context.Accounts.Add(account);

                context.SaveChanges();

                // update
                account.RowVersion = 100500;
                account.Login = "second";

                context.SaveChangesIgnoreConcurrency();

                context.Entry(account).Reload();

                Assert.AreEqual("second", account.Login);
                Assert.AreNotEqual(100500, account.RowVersion);
            }
        }

        [TestMethod]
        public async Task TestSaveChangesIgnoreConcurrencyAsync()
        {
            using (var context = new TestDbContext(_connection))
            {
                // insert
                var account = new Account { Login = "first" };
                context.Accounts.Add(account);

                await context.SaveChangesAsync();

                // update
                account.RowVersion = 100500;
                account.Login = "second";

                await context.SaveChangesIgnoreConcurrencyAsync();

                context.Entry(account).Reload();

                Assert.AreEqual("second", account.Login);
                Assert.AreNotEqual(100500, account.RowVersion);
            }
        }
    }
}
