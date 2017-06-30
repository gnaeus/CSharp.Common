using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Extensions
{
	public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source) {
                action(item);
            }
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static IEnumerable<TItem> DistinctBy<TItem, TKey>(
            this IEnumerable<TItem> source, Func<TItem, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            return source.Where(element => knownKeys.Add(keySelector(element)));
        }

        public static IEnumerable<T> OmitRepeated<T>(this IEnumerable<T> source)
            where T : IEquatable<T>
        {
            T last;
            var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext()) {
                yield return last = enumerator.Current;
            } else {
                yield break;
            }
            while (enumerator.MoveNext()) {
                if (!last.Equals(enumerator.Current)) {
                    yield return enumerator.Current;
                }
                last = enumerator.Current;
            }
        }

        public static IEnumerable<TItem> OmitRepeatedBy<TItem, TKey>(
            this IEnumerable<TItem> source, Func<TItem, TKey> keySelector)
            where TKey : IEquatable<TKey>
        {
            TKey lastKey;
            var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext()) {
                lastKey = keySelector(enumerator.Current);
                yield return enumerator.Current;
            } else {
                yield break;
            }
            while (enumerator.MoveNext()) {
                TKey currentKey = keySelector(enumerator.Current);
                if (!lastKey.Equals(currentKey)) {
                    yield return enumerator.Current;
                }
                lastKey = currentKey;
            }
        }
    }
}
