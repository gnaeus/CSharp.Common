using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using FastExpressionCompiler;

namespace Common.Utils
{
    public class IndexedCollection<T> : ICollection<T>
    {
        readonly List<IEqualityIndex<T>> _indexes = new List<IEqualityIndex<T>>();

        readonly Dictionary<T, List<object>> _storage = new Dictionary<T, List<object>>();

        public int Count => _storage.Count;

        public bool IsReadOnly => false;

        public IndexedCollection()
        {
        }

        public IndexedCollection(IEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));

            foreach (T item in enumerable)
            {
                Add(item);
            }
        }
        
        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            List<object> indexKeys;
            if (_storage.TryGetValue(item, out indexKeys))
            {
                for (int i = 0; i < _indexes.Count; i++)
                {
                    IEqualityIndex<T> index = _indexes[i];
                    object currentKey = index.GetKey(item);
                    object lastKey = indexKeys[i];

                    if (lastKey != currentKey)
                    {
                        indexKeys[i] = currentKey;
                        index.Remove(lastKey, item);
                        index.Add(currentKey, item);
                    }
                }
            }
            else
            {
                indexKeys = new List<object>(_indexes.Count);

                foreach (IEqualityIndex<T> index in _indexes)
                {
                    object key = index.GetKey(item);

                    indexKeys.Add(key);
                    index.Add(key, item);
                }

                _storage.Add(item, indexKeys);
            }
        }

        public void Clear()
        {
            foreach (IEqualityIndex<T> index in _indexes)
            {
                _indexes.Clear();
            }
            _storage.Clear();
        }

        public bool Contains(T item)
        {
            return _storage.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _storage.Keys.CopyTo(array, arrayIndex);
        }

        public IEnumerable<T> Filter(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return Filter(predicate.Body);
        }

        private IEnumerable<T> Filter(Expression node)
        {
            var expression = node as BinaryExpression;
            
            if (expression == null)
            {
                throw new NotSupportedException(
                    $"Predicate body {node} should be Binary Expression");
            }

            switch (expression.NodeType)
            {
                case ExpressionType.OrElse:
                    return Filter(expression.Left).Union(Filter(expression.Right));

                case ExpressionType.AndAlso:
                    return Filter(expression.Left).Intersect(Filter(expression.Right));

                case ExpressionType.Equal:
                    return FilterEquality(
                        expression.Left.GetMemberName(),
                        expression.Right.GetValue());

                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return FilterComparsion(
                        expression.Left.GetMemberName(),
                        expression.Right.GetValue(),
                        expression.NodeType);

                default:
                    throw new NotSupportedException(
                        $"Predicate body {node} should be Equality or Comparsion");
            }
        }
        
        private IEnumerable<T> FilterEquality(string memberName, object key)
        {
            IEqualityIndex<T> index = _indexes.FirstOrDefault(i => i.MemberName == memberName);

            if (index == null)
            {
                throw new InvalidOperationException($"There is no index for property '{memberName}'");
            }
            
            return index.Filter(key);
        }

        private IEnumerable<T> FilterComparsion(
            string memberName, object key, ExpressionType type)
        {
            IComparsionIndex<T> index = _indexes
                .OfType<IComparsionIndex<T>>()
                .FirstOrDefault(i => i.MemberName == memberName);

            if (index == null)
            {
                throw new InvalidOperationException($"There is no comparsion index for property '{memberName}'");
            }

            return index.Filter(key, type);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _storage.Keys.GetEnumerator();
        }

        public IndexedCollection<T> IndexBy<TProperty>(
            Expression<Func<T, TProperty>> property, bool isSorted = false)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            IEqualityIndex<T> index;
            if (isSorted)
            {
                index = new ComparsionIndex<T, TProperty>(property);
            }
            else
            {
                index = new EqualityIndex<T, TProperty>(property);
            }
            
            foreach (var pair in _storage)
            {
                T item = pair.Key;
                List<object> indexKeys = pair.Value;

                object key = index.GetKey(item);

                indexKeys.Add(key);
                index.Add(key, item);
            }

            _indexes.Add(index);

            return this;
        }

        public bool Remove(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            List<object> indexKeys;
            if (_storage.TryGetValue(item, out indexKeys))
            {
                for (int i = 0; i < _indexes.Count; i++)
                {
                    IEqualityIndex<T> index = _indexes[i];
                    object lastKey = indexKeys[i];

                    index.Remove(lastKey, item);
                }

                _storage.Remove(item);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Update(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    internal interface IEqualityIndex<T>
    {
        string MemberName { get; }

        IEnumerable<T> Filter(object key);

        object GetKey(T item);

        void Add(object key, T item);

        void Remove(object key, T item);
        
        void Clear();
    }

    internal interface IComparsionIndex<T> : IEqualityIndex<T>
    {
        IEnumerable<T> Filter(object key, ExpressionType type);
    }

    internal abstract class PropertyIndex<T, TProperty>
    {
        public string MemberName { get; }

        readonly Func<T, TProperty> _getKey;

        protected PropertyIndex(Expression<Func<T, TProperty>> lambdaExpression)
        {
            var memberExpression = lambdaExpression.Body as MemberExpression;

            if (memberExpression == null || memberExpression.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException($"Expression {lambdaExpression} is not a Member Access");
            }

            MemberName = memberExpression.Member.Name;

            _getKey = lambdaExpression.CompileFast();
        }

        public object GetKey(T item)
        {
            return _getKey.Invoke(item);
        }
    }

    internal class EqualityIndex<T, TProperty> : PropertyIndex<T, TProperty>, IEqualityIndex<T>
    {
        readonly Dictionary<TProperty, object> _storage = new Dictionary<TProperty, object>();
        
        public EqualityIndex(Expression<Func<T, TProperty>> lambda)
            : base(lambda)
        {    
        }

        public IEnumerable<T> Filter(object key)
        {
            object bucket;
            if (_storage.TryGetValue((TProperty)key, out bucket))
            {
                return bucket is T
                    ? new[] { (T)bucket }
                    : (IEnumerable<T>)bucket;
            }

            return Enumerable.Empty<T>();
        }
        
        public void Add(object key, T item)
        {
            var propKey = (TProperty)key;

            object bucket;
            if (_storage.TryGetValue(propKey, out bucket))
            {
                if (bucket is T)
                {
                    _storage[propKey] = new List<T>(2) { (T)bucket, item };
                }
                else if (bucket is List<T>)
                {
                    var list = (List<T>)bucket;

                    if (list.Count < 16)
                    {
                        list.Add(item);
                    }
                    else
                    {
                        _storage[propKey] = new HashSet<T>(list) { item };
                    }
                }
                else
                {
                    var hashSet = (HashSet<T>)bucket;

                    hashSet.Add(item);
                }
            }
            else
            {
                _storage.Add(propKey, item);
            }
        }

        public void Remove(object key, T item)
        {
            var propKey = (TProperty)key;

            object bucket = _storage[propKey];

            if (bucket is T)
            {
                _storage.Remove(propKey);
            }
            else if (bucket is List<T>)
            {
                var list = (List<T>)bucket;

                list.Remove(item);

                if (list.Count == 1)
                {
                    _storage[propKey] = list[0];
                }
            }
            else
            {
                var hashSet = (HashSet<T>)bucket;

                hashSet.Remove(item);

                if (hashSet.Count == 16)
                {
                    _storage[propKey] = new List<T>(hashSet);
                }
            }
        }
        
        public void Clear()
        {
            _storage.Clear();
        }
    }

    internal class ComparsionIndex<T, TProperty> : PropertyIndex<T, TProperty>, IComparsionIndex<T>
    {
        public ComparsionIndex(Expression<Func<T, TProperty>> lambda)
            : base(lambda)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Filter(object key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Filter(object key, ExpressionType type)
        {
            throw new NotImplementedException();
        }

        public void Add(object key, T item)
        {
            throw new NotImplementedException();
        }
        
        public void Remove(object key, T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }

    public static class EnumerableExtensions
    {
        public static IndexedCollection<TSource> IndexBy<TSource, TProperty>(
            this IEnumerable<TSource> enumerable,
            Expression<Func<TSource, TProperty>> property,
            bool isSorted = false)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (property == null) throw new ArgumentNullException(nameof(property));

            var collection = enumerable as IndexedCollection<TSource>
                ?? new IndexedCollection<TSource>(enumerable);

            return collection.IndexBy(property, isSorted);
        }
    }

    internal static class ExpressionExtensions
    {
        public static object GetValue(this Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            if (expression.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expression).Value;
            }
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var member = (MemberExpression)expression;

                if (member.Expression.NodeType == ExpressionType.Constant)
                {
                    var instance = (ConstantExpression)member.Expression;

                    if (instance.Type.GetTypeInfo().IsDefined(typeof(CompilerGeneratedAttribute)))
                    {
                        return instance.Type
                            .GetField(member.Member.Name)
                            .GetValue(instance.Value);
                    }
                }
            }

            // we can't interpret the expression but we can compile and run it
            var objectMember = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            try
            {
                return getterLambda.Compile().Invoke();
            }
            catch (InvalidOperationException exception)
            {
                throw new NotSupportedException($"Value of {expression} can't be comuted", exception);
            }
        }

        public static string GetMemberName(this Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            if (expression.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException($"Expression {expression} is not a Member Access");
            }
            
            return ((MemberExpression)expression).Member.Name;
        }
    }
}
