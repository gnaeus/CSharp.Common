using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.MemoryCache
{
    internal class LazyValue<T> : Lazy<T>
    {
        public LazyValue(Func<T> valueFactory)
            : base(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }
    }

    internal class LazyTask<T> : Lazy<Task<T>>
    {
        public LazyTask(Func<Task<T>> taskFactory)
            : base(() => Task.Factory.StartNew(taskFactory).Unwrap(),
                   LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }
    }
}
