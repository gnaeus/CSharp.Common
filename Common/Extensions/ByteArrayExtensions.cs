using System;
using System.Linq;
using System.Security.Cryptography;

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

        /// <summary>
        /// Concat multiple ByteArrays.
        /// </summary>
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

        /// <summary>
        /// Sign message with HMAC algorithm.
        /// </summary>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        /// <param name="rawMessage">RAW message</param>
        /// <param name="hmacKey">HMAC key</param>
        /// <returns>HMAC signed message</returns>
        public static byte[] HmacSign(this byte[] rawMessage, byte[] hmacKey)
        {
            if (rawMessage == null)
            {
                throw new ArgumentNullException(nameof(rawMessage));
            }
            if (hmacKey == null)
            {
                throw new ArgumentNullException(nameof(hmacKey));
            }
            if (hmacKey.Length != 16 && hmacKey.Length != 24 && hmacKey.Length != 32)
            {
                throw new ArgumentException("Should have Length = 16, 24 or 32", nameof(hmacKey));
            }
            using (HMACSHA256 hmac = new HMACSHA256(hmacKey))
            {
                return rawMessage.Concat(hmac.ComputeHash(rawMessage));
            }
        }

        /// <summary>
        /// Extract HMAC signed message.
        /// </summary>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="CryptographicException" />
        /// <param name="hmacMessage">HMAC signed message</param>
        /// <param name="hmacKey">HMAC key</param>
        /// <returns>RAW message</returns>
        public static byte[] HmacExtract(this byte[] hmacMessage, byte[] hmacKey)
        {
            if (hmacMessage == null)
            {
                throw new ArgumentNullException(nameof(hmacMessage));
            }
            if (hmacMessage.Length < 32)
            {
                throw new ArgumentException("Should have Length >= 32", nameof(hmacMessage));
            }
            if (hmacKey == null)
            {
                throw new ArgumentNullException(nameof(hmacKey));
            }
            if (hmacKey.Length != 16 && hmacKey.Length != 24 && hmacKey.Length != 32)
            {
                throw new ArgumentException("Should have Length = 16, 24 or 32", nameof(hmacKey));
            }

            byte[] rawMessage = hmacMessage.ExtractBytes(0, hmacMessage.Length - 32);
            byte[] hash = hmacMessage.ExtractBytes(hmacMessage.Length - 32, 32);

            using (HMACSHA256 hmac = new HMACSHA256(hmacKey))
            {
                byte[] realHash = hmac.ComputeHash(rawMessage);
                if (!hash.SequenceEqual(realHash))
                {
                    throw new CryptographicException("Data integrity check failed");
                }
            }
            return rawMessage;
        }
    }
}
