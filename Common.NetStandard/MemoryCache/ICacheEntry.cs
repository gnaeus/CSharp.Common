using System;
using System.Threading.Tasks;

namespace Common.MemoryCache
{
    public interface IBaseEntry
    {
        bool HasDerivedEntries { get; }

        void AddDerivedEntry(ICacheEntry cacheEntry);

        void RemoveDerivedEntry(ICacheEntry cacheEntry);

        void ForEachDerivedEntry(Action<ICacheEntry> action);

        void ScanForRemovedDerivedEntries();
    }

    public interface ICacheEntry : IBaseEntry
    {
        object Key { get; }

        object[] Tags { get; }

        bool IsExpired { get; }

        bool IsRemovedFromStorage { get; }

        void MarkAsRemovedFromStorage();

        T GetValue<T>();

        Task<T> GetTask<T>();
    }
    
    public interface IEntryFactory
    {
        IBaseEntry CreateBase(ICacheEntry cacheEntry);

        ICacheEntry Create<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, T value);

        ICacheEntry Create<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<T> valueFactory);

        ICacheEntry Create<T>(
            object key, object[] tags, bool isSliding, TimeSpan lifetime, Func<Task<T>> taskFactory);
    }
}
