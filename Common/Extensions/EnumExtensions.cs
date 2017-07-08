using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Extensions
{
	public static class EnumExtensions
    {
        /// <summary>
        /// `color.In(Colors.First, Colors.Second)` is equivalent to `color == Colors.First || color == Colors.Second`.
        /// </summary>
        public static bool In<TEnum>(this TEnum value, params TEnum[] values)
            where TEnum : struct, IComparable
        {
            return values.Contains(value);
        }

        /// <summary>
        /// Convert `[Flags] enum` to `Dictionary<TEnum, bool>`.
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public static Dictionary<TEnum, bool> ToDictionary<TEnum>(this TEnum value)
            where TEnum : struct, IComparable
        {
            if (!typeof(TEnum).IsEnum) {
                throw new NotSupportedException(typeof(TEnum).Name);
            }

            var enumValue = (Enum)(object)value;

            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .ToDictionary(flag => flag,
                    flag => enumValue.HasFlag((Enum)(object)flag));
        }

        /// <summary>
        /// Convert nullable `[Flags] enum` to `Dictionary<TEnum, bool>`.
        /// </summary>
        /// <exception cref="NotSupportedException" />
	    public static Dictionary<TEnum, bool> ToDictionary<TEnum>(this TEnum? value)
	        where TEnum : struct, IComparable
	    {
	        if (!typeof (TEnum).IsEnum) {
	            throw new NotSupportedException(typeof (TEnum).Name);
	        }

	        return value != null ? ToDictionary(value.Value) : null;
	    }

        /// <summary>
        /// Convert `Dictionary<TEnum, bool>` to nullable `[Flags] enum`.
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public static TEnum? ToEnum<TEnum>(this IDictionary<TEnum, bool> value)
            where TEnum : struct, IComparable
        {
            if (!typeof (TEnum).IsEnum) {
                throw new NotSupportedException(typeof (TEnum).Name);
            }
            if (value == null) {
                return null;
            }

            IEnumerable<TEnum> flags = value.Where(el => el.Value).Select(el => el.Key);

            dynamic result = default(TEnum);
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (TEnum flag in flags) {
                dynamic dynFlag = flag;
                result |= dynFlag;
            }
            // ReSharper restore LoopCanBeConvertedToQuery
            return result;
        }
    }
}