using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct CardDef
    {
        public CardType type;
        public string description;
        public int amount;
        public int tileIndex;
    }
}
