using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.Extensions;

namespace Common.Jobs
{
    public class AsyncJobsManager
    {
        private int IsStopped = 0;

        private TaskCompletionSource<object> IsStoppedSource = new TaskCompletionSource<object>();

        private readonly SemaphoreSlim RunningCounter = new SemaphoreSlim(0);

        private readonly ConcurrentDictionary<MethodInfo, SemaphoreSlim> AsyncLocks
            = new ConcurrentDictionary<MethodInfo, SemaphoreSlim>();
        
        private SemaphoreSlim GetLock(MethodInfo actionMethod)
        {
            return AsyncLocks.GetOrAdd(actionMethod, _ => new SemaphoreSlim(1, 1));
        }
        
        [SuppressMessage(null, "CS4014")]
        public void Execute(Func<Task> asyncAction)
        {
            ExecuteAsync(asyncAction);
        }

        public async Task ExecuteAsync(Func<Task> asyncAction)
        {
            // await Task.Yield(); without context
            await Task.Run(() => { }).ConfigureAwait(false);
            
            SemaphoreSlim asyncLock = GetLock(asyncAction.Method);
            try {
                await asyncLock.WaitAsync();
                RunningCounter.Release();

                if (IsStopped == 0) {
                    await asyncAction();
                }
            } finally {
                await RunningCounter.WaitAsync();

                if (IsStopped == 1 && RunningCounter.CurrentCount == 0) {
                    IsStoppedSource.TrySetResult(null);
                }
                asyncLock.Release();
            }
        }

        public void Stop()
        {
            StopAsync().AsSyncronous();
        }

        public Task StopAsync()
        {
            Interlocked.Exchange(ref IsStopped, 1);
            if (RunningCounter.CurrentCount == 0) {
                IsStoppedSource.TrySetResult(null);
            }
            return IsStoppedSource.Task;
        }
    }
}
