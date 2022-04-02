using System.Collections;
using System.Text;

namespace Toggle.Utils
{
    public static class BitArrayExtensions
    {
        // 느린 방법이지만.. 일단 써야하므로
        public static int CountSetBits(this BitArray bitArray)
        {
            int setCount = 0;
            for (int i = 0; i < bitArray.Count; i++)
            {
                if (bitArray[i]) setCount++;
            }

            return setCount;
        }

        public static byte[] BitArrayToByteArray(this BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        public static string ToBase64String(this BitArray bits)
        {
            byte[] b = bits.BitArrayToByteArray();
            return System.Convert.ToBase64String(b);

        }

        public static int CountSetSameFromOther(this BitArray a, BitArray b)
        {
            int result = 0;

            if (a.Length != b.Length)
                throw new System.ArgumentException($"Two BitArray must have same length! {a.Length} != {b.Length}");

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == b[i]) result++;
            }
            return result;
        }

        public static bool CompareTo(this BitArray a, BitArray b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
}