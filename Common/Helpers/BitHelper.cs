
namespace Common.Helpers
{
    public static class BitHelper
    {
        /// <summary>
        /// http://zimbry.blogspot.ru/2011/09/better-bit-mixing-improving-on.html
        /// </summary>
        public static ulong MurmurHash3(ulong key)
        {
            key ^= key >> 33;
            key *= 0xff51afd7ed558ccd;
            key ^= key >> 33;
            key *= 0xc4ceb9fe1a85ec53;
            key ^= key >> 33;
            return key;
        }

        /// <summary>
        /// Reverse bits in `[Flags] enum` value for use in `OrderBy()` extension
        /// http://graphics.stanford.edu/~seander/bithacks.html#ReverseParallel
        /// </summary>
        public static uint ReverseBits(uint value)
        {
            // swap odd and even bits
            value = ((value >> 1) & 0x55555555) | ((value & 0x55555555) << 1);
            // swap consecutive pairs
            value = ((value >> 2) & 0x33333333) | ((value & 0x33333333) << 2);
            // swap nibbles ... 
            value = ((value >> 4) & 0x0F0F0F0F) | ((value & 0x0F0F0F0F) << 4);
            // swap bytes
            value = ((value >> 8) & 0x00FF00FF) | ((value & 0x00FF00FF) << 8);
            // swap 2-byte long pairs
            return (value >> 16) | (value << 16);
        }
    }
}
