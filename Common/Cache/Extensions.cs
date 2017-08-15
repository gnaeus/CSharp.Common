using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Cache
{
    public static class Extensions
    {
        public static bool Remove<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            return dictionary.Remove(new KeyValuePair<TKey, TValue>(key, value));
        }

        public static bool Remove<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> pair)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Remove(pair);
        }
    }
}
