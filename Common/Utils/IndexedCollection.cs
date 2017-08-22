using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Common.Utils
{
    public class IndexedCollection<T> : ICollection<T>
    {
        readonly List<object> _indexes;

        readonly Dictionary<T, List<object>> _storage;
        
        public IndexedCollection()
        {
            _indexes = new List<object>();

            _storage = new Dictionary<T, List<object>>();
        }

        public IndexedCollection(IEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));

            _indexes = new List<object>();

            _storage = enumerable.ToDictionary(el => el, _ => new List<object>(1));
        }

        public int Count => _storage.Count;

        public bool IsReadOnly => false;

        public IndexedCollection<T> IndexBy<TProperty>(
            Expression<Func<T, TProperty>> property, bool isUnique = false)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            
            throw new NotImplementedException();
        }

        public IEnumerable<T> Filter(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }
        
        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _storage.Keys.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _storage.Keys.GetEnumerator();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class EnumerableExtensions
    {
        public static IndexedCollection<TSource> IndexBy<TSource, TProperty>(
            this IEnumerable<TSource> enumerable,
            Expression<Func<TSource, TProperty>> property,
            bool isUnique = false)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (property == null) throw new ArgumentNullException(nameof(property));

            var collection = enumerable as IndexedCollection<TSource>
                ?? new IndexedCollection<TSource>(enumerable);

            return collection.IndexBy(property, isUnique);
        }
    }
}
