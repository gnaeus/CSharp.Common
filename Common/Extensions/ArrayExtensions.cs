using System;

namespace Common.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] Add<T>(this T[] array, T item)
        {
            if (array == null) {
                return new[] { item };
            }
            var result = new T[array.Length + 1];
            Array.Copy(array, result, array.Length);
            result[array.Length] = item;
            return result;
        }

        public static T[] Remove<T>(this T[] array, T item)
        {
            if (array == null) {
                return null;
            }
            int index = Array.IndexOf(array, item);
            if (index == -1) {
                return array;
            }
            var result = new T[array.Length - 1];
            Array.Copy(array, 0, result, 0, index);
            Array.Copy(array, index + 1, result, index, array.Length - index - 1);
            return result;
        }

        public static T[] Replace<T>(this T[] array, T oldItem, T newItem)
        {
            if (array == null) {
                return null;
            }
            int index = Array.IndexOf(array, oldItem);
            if (index == -1) {
                return array;
            }
            var result = new T[array.Length];
            Array.Copy(array, 0, result, 0, index);
            result[index] = newItem;
            Array.Copy(array, index + 1, result, index + 1, array.Length - index - 1);
            return result;
        }
    }
}
