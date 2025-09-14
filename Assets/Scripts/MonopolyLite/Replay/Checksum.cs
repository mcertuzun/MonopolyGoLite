namespace MonopolyLite
{
    public static class Checksum
    {
        public static ulong Hash(ref MonopolyLiteGame.State s)
        {
            ulong h = 1469598103934665603UL;

            void Mix(ulong x)
            {
                h ^= x;
                h *= 1099511628211UL;
            }

            Mix(s.frame);
            Mix(s.initialSeed);
            Mix((ulong)s.playerCount);
            Mix((ulong)s.currentPlayer);
            Mix((ulong)s.goPayout);
            for (int i = 0; i < s.playerCount; i++)
            {
                Mix((ulong)(uint)s.pos[i]);
                Mix((ulong)(uint)s.cash[i]);
                Mix((ulong)(uint)s.jailTurns[i]);
                Mix((ulong)(uint)s.doublesInRow[i]);
            }

            for (int t = 0; t < s.tileOwner.Length; t++) Mix((ulong)(uint)(s.tileOwner[t] + 1));
            return h;
        }
    }
}