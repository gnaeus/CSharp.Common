using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using Dapper;
using EntityFramework.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Common.Tests.Extensions
{
    [TestClass]
    public class CollectionMappingTest
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

        public class BlogModel
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
        public void TestCollectionMapping()
        {
            // create
            using (var context = new TestDbContext(_connection))
            {
                context.Blogs.AddRange(new[]
                {
                    new Blog { Theme = "first" },
                    new Blog { Theme = "second" },
                });
                context.SaveChanges();
            }

            // update
            var models = new[]
            {
                new BlogModel { Id = 1, Theme = "first changed" },
                new BlogModel { Theme = "third" },
            };

            using (var context = new TestDbContext(_connection))
            {
                var entities = context.Blogs.ToList();

                context.Blogs.UpdateCollection(entities, models)
                    .WithKeys(e => e.Id, m => m.Id)
                    .MapValues((e, m) =>
                    {
                        e.Theme = m.Theme;
                    });

                context.SaveChanges();
            }

            // read
            using (var context = new TestDbContext(_connection))
            {
                var entities = context.Blogs.ToList();

                Assert.AreEqual(2, entities.Count);

                Assert.AreEqual(1, entities[0].Id);
                Assert.AreEqual("first changed", entities[0].Theme);

                Assert.AreEqual(3, entities[1].Id);
                Assert.AreEqual("third", entities[1].Theme);
            }
        }
    }
}
