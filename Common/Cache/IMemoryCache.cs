using System;
using System.Threading.Tasks;

namespace Common.Cache
{
    public interface IMemoryCache
    {
        bool TryGet<T>(object key, out T value);

        void Add<T>(object key, object[] tags, bool isSliding, TimeSpan lifetime, T value);

        T GetOrAdd<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<T> valueFactory);

        Task<T> GetOrAddAsync<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<Task<T>> taskFactory);

        void Remove(object key);

        void ClearTag(object tag);
    }
}
