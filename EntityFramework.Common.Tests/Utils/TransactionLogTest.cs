using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using SQLite.CodeFirst;
using EntityFramework.Common.Entities;
using EntityFramework.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Common.Tests.Utils
{
    [TestClass]
    public class TransactionLogTest
    {
        private DbConnection _connection;

        [TestInitialize]
        public void TestInitialize()
        {
            _connection = new SQLiteConnection("data source=:memory:");

            _connection.Open();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _connection.Close();
        }

        public class Post : ITransactionLoggable
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
        }

        public class TestDbContext : DbContext
        {
            public DbSet<TransactionLog> TransactionLogs { get; set; }
            public DbSet<Post> Posts { get; set; }

            public TestDbContext(DbConnection connection)
                : base(connection, false) { }


            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<TestDbContext>(modelBuilder);
                Database.SetInitializer(sqliteConnectionInitializer);
            }

            public override int SaveChanges()
            {
                return this.SaveChangesWithTransactionLog(base.SaveChanges);
            }
        }

        [TestMethod]
        public void TestTransactionLogAdded()
        {
            using (var context = new TestDbContext(_connection))
            {
                context.Posts.Add(new Post { Title = "test", Content = "test test test" });

                context.SaveChanges();
            }

            using (var context = new TestDbContext(_connection))
            {
                var post = context.Posts.FirstOrDefault(p => p.Title == "test");
                var transactionLog = context.TransactionLogs.FirstOrDefault();

                Assert.IsNotNull(post);
                Assert.IsNotNull(transactionLog);
            }
        }
    }
}
