using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct HeistResult
    {
        public bool IsMatch;
        public HeistSymbol MatchedSymbol;
        public int CoinsEarned;
        public HeistSymbol[] Grid;
    }
}
