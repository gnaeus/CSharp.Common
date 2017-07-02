using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Helpers
{
    public static class StringInterpolationHelper
    {
        public static string @if(bool condition, string success, string fail = "")
        {
            return condition ? success : fail;
        }

        public static string @else(string content)
        {
            return content;
        }

        public static string @foreach<T>(IEnumerable<T> items, Func<T, string> selector, string sep = "\n")
        {
            return items != null ? String.Join(sep, items.Select(selector)) : "";
        }

        public static string @switch<T>(T value, params Case<T>[] cases)
        {
            return cases?.FirstOrDefault(c => Object.Equals(c.Pattern, value)).Content ?? "";
        }

        public static Case<T> @case<T>(T pattern, string content)
        {
            return new Case<T> { Pattern = pattern, Content = content };
        }

        public struct Case<T>
        {
            public T Pattern;
            public string Content;
        }
    }
}
