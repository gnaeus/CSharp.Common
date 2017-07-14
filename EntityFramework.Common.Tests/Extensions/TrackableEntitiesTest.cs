using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using Dapper;
using EntityFramework.Common.Entities;
using EntityFramework.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Common.Tests.Extensions
{
    [TestClass]
    public class TrackableEntitiesTest
    {
        private DbConnection _connection;

        [TestInitialize]
        public void TestInitialize()
        {
            _connection = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");

            _connection.Open();

            _connection.Execute(@"
                CREATE TABLE Blogs (
                    Id INTEGER PRIMARY KEY,
                    Theme TEXT,
                    IsDeleted BOOLEAN,
                    CreatedUtc DATETIME,
                    UpdatedUtc DATETIME,
                    DeletedUtc DATETIME
                );");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _connection.Close();
        }

        public class Blog : IFullTrackable
        {
            public int Id { get; set; }
            public string Theme { get; set; }

            public bool IsDeleted { get; set; }
            public DateTime CreatedUtc { get; set; }
            public DateTime? UpdatedUtc { get; set; }
            public DateTime? DeletedUtc { get; set; }
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

            public override int SaveChanges()
            {
                this.UpdateTrackableEntities();
                return base.SaveChanges();
            }
        }

        [TestMethod]
        public void TestTrackableEntities()
        {
            using (var context = new TestDbContext(_connection))
            {
                // insert
                var blog = new Blog();
                context.Blogs.Add(blog);

                context.SaveChanges();
                context.Entry(blog).Reload();

                Assert.AreNotEqual(default(DateTime), blog.CreatedUtc);

                // update
                blog.Theme = "test";

                context.SaveChanges();
                context.Entry(blog).Reload();

                Assert.IsNotNull(blog.UpdatedUtc);

                // delete
                context.Blogs.Remove(blog);

                context.SaveChanges();
                context.Entry(blog).Reload();

                Assert.AreEqual(true, blog.IsDeleted);
                Assert.IsNotNull(blog.DeletedUtc);
            }
        }
    }
}
