using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Extensions
{
	public static class EnumerableExtensions
    {
        /// <summary>
        /// Like `List<T>.ForEach(Action<T> action)`.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source) {
                action(item);
            }
        }

        /// <summary>
        /// Create `HashSet<T>` from `IEnumerable<T>`.
        /// </summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        /// <summary>
        /// Like `Distinct()` but uses values from `keySelector` for equality check.
        /// </summary>
        public static IEnumerable<TItem> DistinctBy<TItem, TKey>(
            this IEnumerable<TItem> source, Func<TItem, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            return source.Where(element => knownKeys.Add(keySelector(element)));
        }

        /// <summary>
        /// Remove repeated values from sequence.
        /// </summary>
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

        /// <summary>
        /// Like `OmitRepeated()` but uses values from `keySelector` for equality check.
        /// </summary>
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
