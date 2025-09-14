namespace MonopolyLite
{
    public struct RNG
    {
        private ulong s;
        private ulong inc;

        public RNG(uint seed, uint seq = 54u)
        {
            s = 0UL;
            inc = ((ulong)seq << 1) | 1UL;
            NextUInt();
            s += seed;
            NextUInt();
        }

        public uint NextUInt()
        {
            ulong old = s;
            s = old * 6364136223846793005UL + inc;
            uint x = (uint)(((old >> 18) ^ old) >> 27);
            uint r = (uint)(old >> 59);
            return (x >> (int)r) | (x << (int)(-r & 31));
        }

        public uint Next(uint a, uint b)
        {
            uint range = b - a;
            uint r = NextUInt();
            return a + r % range;
        }

        public int Next(int a, int b)
        {
            return (int)Next((uint)a, (uint)b);
        }
    }
}