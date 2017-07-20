using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EntityFramework.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Common.Tests.Extensions
{
    [TestClass]
    public class ChunkedInQueryTest
    {
        private DbConnection _connection;

        [TestInitialize]
        public void TestInitialize()
        {
            _connection = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");

            _connection.Open();

            _connection.Execute(@"
                CREATE TABLE Notes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Text TEXT
                );");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _connection.Close();
        }

        public class Note
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }
        
        public class TestDbContext : DbContext
        {
            public DbSet<Note> Notes { get; set; }

            public TestDbContext(DbConnection connection)
                : base(connection, false)
            {
                Database.Log = s => Debug.WriteLine(s);
                Database.SetInitializer<TestDbContext>(null);
            }
        }

        [TestMethod]
        public async Task TestChunkedInQuery()
        {
            var insetredNotes = Enumerable.Range(0, 4000)
                .Select(i => new Note { Text = $"test {i}" });

            _connection.Execute("INSERT INTO Notes (Text) VALUES (@Text)", insetredNotes);

            using (var context = new TestDbContext(_connection))
            {
                var notes = await context.Notes
                    .ExecuteChunkedInQueryAsync(n => n.Id, Enumerable.Range(1, 4000));

                Assert.AreEqual(4000, notes.Count);
            }
        }
    }
}
