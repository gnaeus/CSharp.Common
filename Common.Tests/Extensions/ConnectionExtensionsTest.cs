using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Extensions;

namespace Common.Tests.Extensions
{
    [TestClass]
    public class ConnectionExtensionsTest
    {
        [TestMethod]
        public void TestConnectionAlreadyOpen()
        {
            var conn = new SQLiteConnection("DataSource=:memory:");
            conn.Open();

            using (conn.EnsureOpen())
            using (var t = conn.BeginTransaction()) {
                t.Commit();
            }

            Assert.AreEqual(ConnectionState.Open, conn.State);
            
            conn.Close();
        }

        [TestMethod]
        public async Task TestConnectionAlreadyOpenAsync()
        {
            var conn = new SQLiteConnection("DataSource=:memory:");
            conn.Open();

            using (await conn.EnsureOpenAsync())
            using (var t = conn.BeginTransaction()) {
                t.Commit();
            }

            Assert.AreEqual(ConnectionState.Open, conn.State);

            conn.Close();
        }

        [TestMethod]
        public void TestConnectionClosing()
        {
            var conn = new SQLiteConnection("DataSource=:memory:");
            
            using (conn.EnsureOpen())
            using (var t = conn.BeginTransaction()) {
                t.Commit();
            }

            Assert.AreEqual(ConnectionState.Closed, conn.State);
        }

        [TestMethod]
        public async Task TestConnectionClosingAsync()
        {
            var conn = new SQLiteConnection("DataSource=:memory:");

            using (await conn.EnsureOpenAsync())
            using (var t = conn.BeginTransaction()) {
                t.Commit();
            }

            Assert.AreEqual(ConnectionState.Closed, conn.State);
        }
    }
}
