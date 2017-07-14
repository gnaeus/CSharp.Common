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
    public class AuditableEntitiesTest
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
                    Theme TEXT,
                    IsDeleted BOOLEAN,
                    CreatedUtc DATETIME,
                    CreatorUserId INTEGER,
                    UpdatedUtc DATETIME,
                    UpdaterUserId INTEGER,
                    DeletedUtc DATETIME,
                    DeleterUserId INTEGER
                );

                CREATE TABLE Posts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Content TEXT,
                    IsDeleted BOOLEAN,
                    CreatedUtc DATETIME,
                    CreatorUser TEXT,
                    UpdatedUtc DATETIME,
                    UpdaterUser TEXT,
                    DeletedUtc DATETIME,
                    DeleterUser TEXT
                );");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _connection.Close();
        }
        
        public class Blog : IFullAuditable<int>
        {
            public int Id { get; set; }
            public string Theme { get; set; }

            public bool IsDeleted { get; set; }
            public DateTime CreatedUtc { get; set; }
            public int CreatorUserId { get; set; }
            public DateTime? UpdatedUtc { get; set; }
            public int? UpdaterUserId { get; set; }
            public DateTime? DeletedUtc { get; set; }
            public int? DeleterUserId { get; set; }
        }

        public class Post : IFullAuditable
        {
            public int Id { get; set; }
            public string Content { get; set; }

            public bool IsDeleted { get; set; }
            public DateTime CreatedUtc { get; set; }
            public string CreatorUser { get; set; }
            public DateTime? UpdatedUtc { get; set; }
            public string UpdaterUser { get; set; }
            public DateTime? DeletedUtc { get; set; }
            public string DeleterUser { get; set; }
        }

        public class TestDbContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Post> Posts { get; set; }

            public TestDbContext(DbConnection connection)
                : base(connection, false)
            {
                Database.Log = s => Debug.WriteLine(s);
                Database.SetInitializer<TestDbContext>(null);
            }
            
            public int SaveChanges(string editorUser)
            {
                this.UpdateAuditableEntities(editorUser);
                return base.SaveChanges();
            }

            public int SaveChanges(int editorUserId)
            {
                this.UpdateAuditableEntities(editorUserId);
                return base.SaveChanges();
            }
        }
        
        [TestMethod]
        public void TestAuditableEntitiesGeneric()
        {
            using (var context = new TestDbContext(_connection))
            {
                int editorUserId = 1000;

                // insert
                var blog = new Blog();
                context.Blogs.Add(blog);

                context.SaveChanges(editorUserId);
                context.Entry(blog).Reload();

                Assert.AreEqual(editorUserId, blog.CreatorUserId);
                Assert.AreNotEqual(default(DateTime), blog.CreatedUtc);

                // update
                blog.Theme = "test";

                context.SaveChanges(editorUserId);
                context.Entry(blog).Reload();

                Assert.AreEqual(editorUserId, blog.UpdaterUserId);
                Assert.IsNotNull(blog.UpdatedUtc);

                // delete
                context.Blogs.Remove(blog);

                context.SaveChanges(editorUserId);
                context.Entry(blog).Reload();

                Assert.AreEqual(true, blog.IsDeleted);
                Assert.AreEqual(editorUserId, blog.DeleterUserId);
                Assert.IsNotNull(blog.DeletedUtc);
            }
        }

        [TestMethod]
        public void TestAuditableEntitiesString()
        {
            using (var context = new TestDbContext(_connection))
            {
                string editorUser = "admin";

                // insert
                var post = new Post();
                context.Posts.Add(post);

                context.SaveChanges(editorUser);
                context.Entry(post).Reload();

                Assert.AreEqual(editorUser, post.CreatorUser);
                Assert.AreNotEqual(default(DateTime), post.CreatedUtc);

                // update
                post.Content = "test";

                context.SaveChanges(editorUser);
                context.Entry(post).Reload();

                Assert.AreEqual(editorUser, post.UpdaterUser);
                Assert.IsNotNull(post.UpdatedUtc);

                // delete
                context.Posts.Remove(post);

                context.SaveChanges(editorUser);
                context.Entry(post).Reload();

                Assert.AreEqual(true, post.IsDeleted);
                Assert.AreEqual(editorUser, post.DeleterUser);
                Assert.IsNotNull(post.DeletedUtc);
            }
        }
    }
}
