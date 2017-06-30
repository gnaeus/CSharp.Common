using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Extensions
{
	public static class EnumExtensions
    {
        public static bool In<T>(this T value, params T[] values)
            where T : struct, IComparable
        {
            return values.Contains(value);
        }

        public static Dictionary<T, bool> ToDictionary<T>(this T value)
            where T : struct, IComparable
        {
            if (!typeof(T).IsEnum) {
                throw new NotSupportedException(typeof(T).Name);
            }

            var enumValue = (Enum)(object)value;

            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToDictionary(flag => flag,
                    flag => enumValue.HasFlag((Enum)(object)flag));
        }

	    public static Dictionary<T, bool> ToDictionary<T>(this T? value)
	        where T : struct, IComparable
	    {
	        if (!typeof (T).IsEnum) {
	            throw new NotSupportedException(typeof (T).Name);
	        }

	        return value != null ? ToDictionary(value.Value) : null;
	    }

        public static T? ToEnum<T>(this IDictionary<T, bool> value)
            where T : struct, IComparable
        {
            if (!typeof (T).IsEnum) {
                throw new NotSupportedException(typeof (T).Name);
            }
            if (value == null) {
                return null;
            }

            IEnumerable<T> flags = value.Where(el => el.Value).Select(el => el.Key);

            dynamic result = default(T);
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (T flag in flags) {
                dynamic dynFlag = flag;
                result |= dynFlag;
            }
            // ReSharper restore LoopCanBeConvertedToQuery
            return result;
        }
    }
}