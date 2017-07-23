using System;
using System.Threading.Tasks;

namespace Common.MemoryCache
{
    public interface IConcurrentCache
    {
        bool TryGetValue<T>(object key, out T value);

        void AddOrUpdate<T>(object key, object[] tags, bool isSliding, TimeSpan lifetime, T value);

        T GetOrAdd<T>(object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<T> valueFactory);

        Task<T> GetOrAddAsync<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<Task<T>> taskFactory);

        void Remove(object key);
    }
}
