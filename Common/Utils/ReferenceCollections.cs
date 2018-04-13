using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Common.Utils
{
    internal class ReferenceEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
    {
        public bool Equals(T x, T y)
        {
            return Equals((object)x, y);
        }

        public new bool Equals(object x, object y)
        {
            switch (x)
            {
                case IStructuralEquatable xTuple:
                    return xTuple.Equals(y, this);

                case ValueType xValue:
                    return xValue.Equals(y);

                default:
                    return ReferenceEquals(x, y);
            }
        }

        public int GetHashCode(T obj)
        {
            return GetHashCode((object)obj);
        }

        public int GetHashCode(object obj)
        {
            switch (obj)
            {
                case IStructuralEquatable tuple:
                    return tuple.GetHashCode(this);

                case ValueType value:
                    return value.GetHashCode();

                default:
                    return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }

    /// <summary>
    /// <see cref="Dictionary{TKey, TValue}"/> that always use <see cref="RuntimeHelpers.GetHashCode"/>
    /// and <see cref="Object.ReferenceEquals(object, object)"/> for classes
    /// and <see cref="IStructuralEquatable"/> for <see cref="Tuple"/> and <see cref="ValueTuple"/>
    /// </summary>
    public class ReferenceDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private static ReferenceEqualityComparer<TKey> EqualityComparer = new ReferenceEqualityComparer<TKey>();

        public ReferenceDictionary()
            : base(EqualityComparer)
        {
        }

        public ReferenceDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary, EqualityComparer)
        {
        }
    }

    /// <summary>
    /// <see cref="HashSet{T}"/> that always use <see cref="RuntimeHelpers.GetHashCode"/>
    /// and <see cref="Object.ReferenceEquals(object, object)"/> for classes
    /// and <see cref="IStructuralEquatable"/> for <see cref="Tuple"/> and <see cref="ValueTuple"/>
    /// </summary>
    public class ReferenceHashSet<T> : HashSet<T>
    {
        private static ReferenceEqualityComparer<T> EqualityComparer = new ReferenceEqualityComparer<T>();

        public ReferenceHashSet()
            : base(EqualityComparer)
        {
        }

        public ReferenceHashSet(IEnumerable<T> collection)
            : base(collection, EqualityComparer)
        {
        }
    }
}
