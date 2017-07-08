using System;
using System.Security.Cryptography;

namespace Common.Utils
{
    /// <summary>
    /// Random class replacement with same API but with usage of RNGCryptoServiceProvider inside.
    /// </summary>
    /// <remarks> https://msdn.microsoft.com/en-us/magazine/cc163367.aspx </remarks>
    public class CryptoRandom : Random
    {
        private readonly byte[] _uint32Buffer = new byte[4];

        private readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

        public override int Next()
        {
            _rng.GetBytes(_uint32Buffer);
            return BitConverter.ToInt32(_uint32Buffer, 0) & 0x7FFFFFFF;
        }

        /// <exception cref="ArgumentOutOfRangeException" />
        public override int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue");
            }
            return Next(0, maxValue);
        }

        /// <exception cref="ArgumentOutOfRangeException" />
        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue");
            }
            if (minValue == maxValue)
            {
                return minValue;
            }
            long diff = maxValue - minValue;

            while (true)
            {
                _rng.GetBytes(_uint32Buffer);
                UInt32 rand = BitConverter.ToUInt32(_uint32Buffer, 0);
                const long max = (1 + (long)UInt32.MaxValue);
                long remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (int)(minValue + (rand % diff));
                }
            }
        }

        public override double NextDouble()
        {
            _rng.GetBytes(_uint32Buffer);
            UInt32 rand = BitConverter.ToUInt32(_uint32Buffer, 0);
            return rand / (1.0 + UInt32.MaxValue);
        }

        /// <exception cref="ArgumentNullException" />
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            _rng.GetBytes(buffer);
        }
    }
}
