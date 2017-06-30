using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common.Tests
{
    public static class TestHelper
    {
        public static string ToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static byte[] FromHex(string hex)
        {
            return Enumerable
                .Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static bool DictionariesAreEqual(IEnumerable firstDict, IEnumerable secondDict)
        {
            var firstSet = new HashSet<KeyValuePair<object, object>>();
            var secondSet = new HashSet<KeyValuePair<object, object>>();

            foreach (dynamic entry in firstDict) {
                firstSet.Add(new KeyValuePair<object, object>(entry.Key, entry.Value));
            }
            foreach (dynamic entry in secondDict) {
                secondSet.Add(new KeyValuePair<object, object>(entry.Key, entry.Value));
            }

            return firstSet.SetEquals(secondSet);
        }

        public static byte[] GetStreamBytes(Stream stream)
        {
            using (var ms = new MemoryStream()) {
                stream.CopyTo(ms);
                if (stream.CanSeek) {
                    stream.Position = 0;
                }
                return ms.ToArray();
            }
        }

        public static string GetStreamHex(Stream stream)
        {
            return ToHex(GetStreamBytes(stream));
        }

        public static string GetStreamString(Stream stream, Encoding encoding)
        {
            return encoding.GetString(GetStreamBytes(stream));
        }
    }
}
