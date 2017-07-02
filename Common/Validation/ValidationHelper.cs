using System;

namespace Common.Validation
{
    internal static class ValidationHelper
    {
        internal static bool IsSimpleType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimpleType(type.GetGenericArguments()[0]);
            }

            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal);
        }

        internal static string CombinePath(string path, string prefix)
        {
            if (String.IsNullOrEmpty(path))
            {
                return prefix ?? "";
            }
            if (String.IsNullOrEmpty(prefix))
            {
                return path ?? "";
            }
            if (Char.IsLetter(prefix[0]) || prefix[0] == '_')
            {
                return path + "." + prefix;
            }
            return path + prefix;
        }

        internal static string MakeDictionaryPath(string path, object key)
        {
            bool isInt = key is sbyte
                || key is byte
                || key is short
                || key is ushort
                || key is int
                || key is uint
                || key is long
                || key is ulong;

            return isInt
                ? $"{path}[{key}]"
                : $"{path}[\"{key}\"]";
        }
    }
}
