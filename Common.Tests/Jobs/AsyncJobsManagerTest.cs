using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Jobs;

namespace Common.Tests.Jobs
{
    [TestClass]
    public class AsyncJobsManagerTest
    {
        [TestMethod]
        public async Task TestQueuedJobs()
        {
            var manager = new AsyncJobsManager();

            int fooCalls = 0, barCalls = 0;
            Func<Task> foo = async () => { await Task.Delay(200); fooCalls++; };
            Func<Task> bar = async () => { await Task.Delay(300); barCalls++; };

            for (int i = 0; i < 5; ++i) {
                manager.Execute(foo);
                manager.Execute(bar);
            }

            await Task.Delay(500);
            manager.Stop();

            Assert.AreEqual(3, fooCalls);
            Assert.AreEqual(2, barCalls);
        }

        [TestMethod]
        public async Task TestCompletedJobs()
        {
            var manager = new AsyncJobsManager();

            int fooCalls = 0, barCalls = 0;
            Func<Task> foo = async () => { await Task.Delay(20); fooCalls++; };
            Func<Task> bar = async () => { await Task.Delay(30); barCalls++; };

            for (int i = 0; i < 5; ++i) {
                manager.Execute(foo);
                manager.Execute(bar);
            }

            await Task.Delay(300);
            manager.Stop();

            Assert.AreEqual(5, fooCalls);
            Assert.AreEqual(5, barCalls);
        }
    }
}
