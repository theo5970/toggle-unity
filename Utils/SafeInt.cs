using System.Runtime.InteropServices;
using Random = System.Random;


namespace Toggle.Utils
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct SafeInt
    {
        private static Random random = new Random();

        [FieldOffset(0)] private byte byte0;

        [FieldOffset(1)] private byte byte1;

        [FieldOffset(2)] private byte byte2;

        [FieldOffset(3)] private byte byte3;

        [FieldOffset(0)]
        private int intValue;

        [FieldOffset(5)]
        private int xorKey;

        public int Value
        {
            get
            {
                Swap(ref byte0, ref byte1);
                Swap(ref byte2, ref byte3);
                int result = intValue ^ xorKey;
            
                xorKey = random.Next();
                intValue = result ^ xorKey;
                Swap(ref byte0, ref byte1);
                Swap(ref byte2, ref byte3);
                return result;
            }
            set
            {
                xorKey = random.Next();
                intValue = value ^ xorKey;
                Swap(ref byte0, ref byte1);
                Swap(ref byte2, ref byte3);
            }
        }

        private void Swap(ref byte a, ref byte b)
        {
            byte tmp = a;
            a = b;
            b = tmp;
        }

        public SafeInt(byte byte0, byte byte1, byte byte2, byte byte3)
        {
            this.intValue = 0;
            this.xorKey = random.Next();
            this.byte0 = byte0;
            this.byte1 = byte1;
            this.byte2 = byte2;
            this.byte3 = byte3;

            Value = intValue;
        }
    
        public SafeInt(int initValue)
        {
            this.xorKey = random.Next();
            this.byte0 = this.byte1 = this.byte2 = this.byte3 = 0;
            this.intValue = 0;

            Value = initValue;
        }
    

        public string GetMemoryAlignment()
        {
            return $"({byte0:X2},{byte1:X2},{byte2:X2},{byte3:X2})";
        }
        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator int(SafeInt value)
        {
            return value.Value;
        }

        public static implicit operator SafeInt(int value)
        {
            return new SafeInt(value);
        }
    }
}