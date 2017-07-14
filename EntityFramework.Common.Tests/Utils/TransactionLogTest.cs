using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using EntityFramework.Common.Entities;
using EntityFramework.Common.Extensions;
using EntityFramework.Common.Utils;
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
            _connection = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");

            _connection.Open();

            _connection.Execute(@"
                CREATE TABLE _TransactionLog (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TransactionId BLOB,
                    CreatedUtc DATETIME,
                    Operation TEXT,
                    SchemaName TEXT,
                    TableName TEXT,
                    EntityType TEXT,
                    EntityJson TEXT
                );

                CREATE TABLE Blogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OwnerId INTEGER,
                    CategoryColumn TEXT
                );

                CREATE TABLE Posts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    BlogId INTEGER,
                    Title TEXT,
                    Content TEXT,
                    TagsJson TEXT
                );");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _connection.Close();
        }

        public class EntityBase
        {
            public int OwnerId { get; set; }
        }

        public class Blog : EntityBase, ITransactionLoggable
        {
            public int Id { get; set; }

            [Column("CategoryColumn")]
            public string Category { get; set; }

            public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
        }

        public class Post : ITransactionLoggable
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }

            public virtual Blog Blog { get; set; }

            JsonField<ICollection<string>> _tags = new HashSet<string>();
            public bool ShouldSerializeTagsJson() => false;
            public string TagsJson
            {
                get { return _tags.Json; }
                set { _tags.Json = value; }
            }
            public ICollection<string> Tags
            {
                get { return _tags.Value; }
                set { _tags.Value = value; }
            }
        }

        public class TestDbContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Post> Posts { get; set; }
            public DbSet<TransactionLog> TransactionLogs { get; set; }

            public TestDbContext(DbConnection connection)
                : base(connection, false)
            {
                Database.Log = s => Debug.WriteLine(s);
                Database.SetInitializer<TestDbContext>(null);
            }

            public override int SaveChanges()
            {
                return this.SaveChangesWithTransactionLog(base.SaveChanges);
            }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
            {
                return this.SaveChangesWithTransactionLogAsync(base.SaveChangesAsync, cancellationToken);
            }

            protected override void OnModelCreating(DbModelBuilder mb)
            {
                mb.UseTransactionLog();

                mb.Entity<Blog>()
                    .HasMany(b => b.Posts)
                    .WithRequired(p => p.Blog)
                    .HasForeignKey(p => p.BlogId)
                    .WillCascadeOnDelete();
            }

            public void OriginalSaveChanges()
            {
                base.SaveChanges();
            }
        }

        [TestMethod]
        public void TestTransactionLogInsert()
        {
            using (var context = new TestDbContext(_connection))
            {
                var post = new Post { Title = "test", Content = "test test test" };

                post.Tags.Add("first");
                post.Tags.Add("second");

                context.Posts.Add(post);

                var blog = new Blog { OwnerId = 5, Category = "tests" };

                context.Blogs.Add(blog);

                blog.Posts.Add(post);

                context.SaveChanges();
            }

            using (var context = new TestDbContext(_connection))
            {
                var logs = context.TransactionLogs.ToArray();

                Assert.IsNotNull(logs);
                Assert.AreEqual(2, logs.Length);
                Assert.AreEqual(logs[0].TransactionId, logs[1].TransactionId);
                Assert.AreEqual(logs[0].CreatedUtc, logs[1].CreatedUtc);

                Assert.AreEqual("Posts", logs[0].TableName);
                Assert.AreEqual(TransactionLog.INSERT, logs[0].Operation);
                Assert.IsInstanceOfType(logs[0].Entity, typeof(Post));
                Assert.IsTrue(logs[0].GetEntity<Post>().Tags.SequenceEqual(new[] { "first", "second" }));

                Assert.AreEqual("Blogs", logs[1].TableName);
                Assert.AreEqual(TransactionLog.INSERT, logs[1].Operation);
                Assert.IsInstanceOfType(logs[1].Entity, typeof(Blog));
                Assert.AreEqual(5, logs[1].GetEntity<Blog>().OwnerId);
                Assert.AreEqual("tests", logs[1].GetEntity<Blog>().Category);
            }
        }

        [TestMethod]
        public void TestTransactionLogUpdate()
        {
            using (var context = new TestDbContext(_connection))
            {
                var blog = new Blog { OwnerId = 5, Category = "tests" };
                blog.Posts.Add(new Post { Title = "test", Content = "test test test" });

                context.Blogs.Add(blog);

                context.OriginalSaveChanges();
            }

            using (var context = new TestDbContext(_connection))
            {
                var blog = context.Blogs.Include(b => b.Posts).FirstOrDefault();

                blog.Category = "modified";
                blog.Posts.First().Tags.Add("modified");

                context.SaveChanges();
            }

            using (var context = new TestDbContext(_connection))
            {
                var logs = context.TransactionLogs.ToArray();

                Assert.IsNotNull(logs);
                Assert.AreEqual(2, logs.Length);
                Assert.AreEqual(logs[0].TransactionId, logs[1].TransactionId);
                Assert.AreEqual(logs[0].CreatedUtc, logs[1].CreatedUtc);

                Assert.AreEqual("Blogs", logs[0].TableName);
                Assert.AreEqual(TransactionLog.UPDATE, logs[0].Operation);
                Assert.IsInstanceOfType(logs[0].Entity, typeof(Blog));
                Assert.AreEqual(5, logs[0].GetEntity<Blog>().OwnerId);
                Assert.AreEqual("modified", logs[0].GetEntity<Blog>().Category);

                Assert.AreEqual("Posts", logs[1].TableName);
                Assert.AreEqual(TransactionLog.UPDATE, logs[1].Operation);
                Assert.IsInstanceOfType(logs[1].Entity, typeof(Post));
                Assert.IsTrue(logs[1].GetEntity<Post>().Tags.SequenceEqual(new[] { "modified" }));
            }
        }

        [TestMethod]
        public void TestTransactionLogDelete()
        {
            using (var context = new TestDbContext(_connection))
            {
                var blog = new Blog { OwnerId = 5, Category = "tests" };
                blog.Posts.Add(new Post { Title = "test", Content = "test test test" });

                context.Blogs.Add(blog);

                context.OriginalSaveChanges();
            }

            using (var context = new TestDbContext(_connection))
            {
                var blog = context.Blogs.Include(b => b.Posts).FirstOrDefault();

                context.Blogs.Remove(blog);

                context.SaveChanges();
            }

            using (var context = new TestDbContext(_connection))
            {
                var logs = context.TransactionLogs.ToArray();

                Assert.IsNotNull(logs);
                Assert.AreEqual(2, logs.Length);
                Assert.AreEqual(logs[0].TransactionId, logs[1].TransactionId);
                Assert.AreEqual(logs[0].CreatedUtc, logs[1].CreatedUtc);

                Assert.AreEqual("Posts", logs[0].TableName);
                Assert.AreEqual(TransactionLog.DELETE, logs[0].Operation);
                Assert.IsInstanceOfType(logs[0].Entity, typeof(Post));

                Assert.AreEqual("Blogs", logs[1].TableName);
                Assert.AreEqual(TransactionLog.DELETE, logs[1].Operation);
                Assert.IsInstanceOfType(logs[1].Entity, typeof(Blog));
            }
        }

        [TestMethod]
        public async Task TestTransactionLogCombined()
        {
            using (var context = new TestDbContext(_connection))
            {
                var blog = new Blog { OwnerId = 5, Category = "tests" };
                blog.Posts.Add(new Post { Title = "first" });
                blog.Posts.Add(new Post { Title = "second" });

                context.Blogs.Add(blog);

                context.OriginalSaveChanges();
            }

            using (var context = new TestDbContext(_connection))
            {
                var blog = context.Blogs.Include(b => b.Posts).FirstOrDefault();

                // insert third post
                blog.Posts.Add(new Post { Title = "third" });

                // update blog and first post
                blog.Category = "modified";
                blog.Posts.First().Tags.Add("modified");

                // delete second post
                context.Posts.Remove(blog.Posts.ElementAt(1));

                await context.SaveChangesAsync();
            }

            using (var context = new TestDbContext(_connection))
            {
                var logs = context.TransactionLogs.ToArray();

                Assert.IsNotNull(logs);
                Assert.AreEqual(4, logs.Length);
                Assert.IsTrue(logs.All(l => l.TransactionId == logs[0].TransactionId));
                Assert.IsTrue(logs.All(l => l.CreatedUtc == logs[0].CreatedUtc));

                Assert.AreEqual("dbo", logs[0].SchemaName);
                Assert.AreEqual("Posts", logs[0].TableName);
                Assert.AreEqual(TransactionLog.INSERT, logs[0].Operation);
                Assert.IsInstanceOfType(logs[0].Entity, typeof(Post));

                Assert.AreEqual("dbo", logs[1].SchemaName);
                Assert.AreEqual("Blogs", logs[1].TableName);
                Assert.AreEqual(TransactionLog.UPDATE, logs[1].Operation);
                Assert.IsInstanceOfType(logs[1].Entity, typeof(Blog));

                Assert.AreEqual("dbo", logs[2].SchemaName);
                Assert.AreEqual("Posts", logs[2].TableName);
                Assert.AreEqual(TransactionLog.UPDATE, logs[2].Operation);
                Assert.IsInstanceOfType(logs[2].Entity, typeof(Post));

                Assert.AreEqual("dbo", logs[3].SchemaName);
                Assert.AreEqual("Posts", logs[3].TableName);
                Assert.AreEqual(TransactionLog.DELETE, logs[3].Operation);
                Assert.IsInstanceOfType(logs[3].Entity, typeof(Post));
            }
        }
    }
}
