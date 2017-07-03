using System;
using System.Text.RegularExpressions;

namespace Common.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex WhiteSpace = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Replace all long white space inside string by one space character.
        /// </summary>
        public static string TrimWhiteSpace(this string input)
        {
            return WhiteSpace.Replace(input, " ");
        }

        /// <summary>
        /// Check if string is Base64 string.
        /// value.Length % 4 == 0 and value is ^[A-Za-z0-9\+/]+={0,2}$
        /// </summary>
        public static bool IsBase64(this string value)
        {
            if (String.IsNullOrEmpty(value) || value.Length % 4 != 0) {
                return false;
            }

            int length = value.Length;
            if (value[length - 1] == 61) {
                --length;
            }
            if (value[length - 1] == 61) {
                --length;
            }

            for (int i = 0; i < length; ++i) {
                int c = value[i];
                if (c < 43 || c > 43 && c < 47 || c > 57 && c < 65 || c > 122) {
                    return false;
                }
            }
            return true;
        }
    }
}