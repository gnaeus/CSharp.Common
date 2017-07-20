using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using Dapper;
using EntityFramework.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Common.Tests.Extensions
{
    [TestClass]
    public class GetTableAndSchemaNameTest
    {
        private DbConnection _connection;

        [TestInitialize]
        public void TestInitialize()
        {
            _connection = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");

            _connection.Open();

            _connection.Execute(@"
                CREATE TABLE Blogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Theme TEXT
                );");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _connection.Close();
        }

        public class Blog
        {
            public int Id { get; set; }
            public string Theme { get; set; }
        }

        public class TestDbContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }

            public TestDbContext(DbConnection connection)
                : base(connection, false)
            {
                Database.Log = s => Debug.WriteLine(s);
                Database.SetInitializer<TestDbContext>(null);
            }
        }

        [TestMethod]
        public void TestGetTableName()
        {
            using (var context = new TestDbContext(_connection))
            {
                var tableAndSchema = context.GetTableAndSchemaName(typeof(Blog));

                Assert.AreEqual("Blogs", tableAndSchema.TableName);
                Assert.AreEqual("dbo", tableAndSchema.Schema);
            }
        }
    }
}
