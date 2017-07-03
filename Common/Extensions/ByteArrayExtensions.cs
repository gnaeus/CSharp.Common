using System;
using System.Linq;

namespace Common.Extensions
{
    public static class ByteArrayExtensions
    {
        private static int MemCmp(byte[] first, byte[] second, UIntPtr count)
        {
            uint length = count.ToUInt32();
            for (uint i = 0; i < length; ++i) {
                if (first[i] != second[i]) {
                    return 1;
                }
            }
            return 0;
        }
        
        public static bool SequenceEqual(this byte[] first, byte[] second)
        {
            // reference equality check
            if (first == second) {
                return true; 
            }
            if (first == null || second == null || first.Length != second.Length) {
                return false;
            }
            return MemCmp(first, second, new UIntPtr((uint)first.Length)) == 0;
        }

        public static byte[] ExtractBytes(this byte[] source, int offset, int count)
        {
            var result = new byte[count];
            Buffer.BlockCopy(source, offset, result, 0, count);
            return result;
        }

        public static byte[] Concat(this byte[] first, byte[] second)
        {
            var result = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, result, 0, first.Length);
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
            return result;
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            var result = new byte[arrays.Sum(a => a.Length)];

            int offset = 0;
            foreach (byte[] array in arrays) {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }
            return result;
        }
    }
}
