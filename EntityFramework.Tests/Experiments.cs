using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;

namespace EntityFramework.Common.Tests
{
    [TestClass]
    public class Experiments
    {
        private DbConnection _connection;

        [TestInitialize]
        public void TestInitialize()
        {
            _connection = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");

            _connection.Open();

            _connection.Execute(@"
                CREATE TABLE Entities (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Data TEXT
                );

                CREATE TABLE EntitiesHistory (
                    HistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
                    Id INTEGER,
                    Data TEXT
                );");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _connection.Close();
        }

        public class EntityTable
        {
            [Key]
            public int Id { get; set; }
            public string Data { get; set; }
        }

        public class Entity : EntityTable
        {
        }

        public class EntityHistory : EntityTable
        {
            public long HistoryId { get; set; }
        }

        public class TestDbContext : DbContext
        {
            public DbSet<Entity> Entities { get; set; }
            public DbSet<EntityHistory> EntitiesHistory { get; set; }

            public TestDbContext(DbConnection connection)
                : base(connection, false)
            {
                Database.Log = s => Debug.WriteLine(s);
                Database.SetInitializer<TestDbContext>(null);
            }

            protected override void OnModelCreating(DbModelBuilder mb)
            {
                mb.Entity<EntityHistory>()
                    .ToTable(nameof(EntitiesHistory));

                mb.Entity<EntityHistory>()
                    .HasKey(h => h.HistoryId);
            }
        }

        [TestMethod]
        public void TestHistory()
        {
            using (var context = new TestDbContext(_connection))
            {
                context.Entities.Add(new Entity { Data = "entity" });

                context.EntitiesHistory.Add(new EntityHistory { Id = 1, Data = "history" });

                context.SaveChanges();
            }

            using (var context = new TestDbContext(_connection))
            {
                var entities = context.Entities.ToList();

                var entitiesHistory = context.EntitiesHistory.ToList();
            }
        }
    }
}