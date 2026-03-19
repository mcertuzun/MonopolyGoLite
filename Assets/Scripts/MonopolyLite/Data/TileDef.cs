using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct TileDef
    {
        public string name;
        public TileType type;
        public ColorGroup colorGroup;
        public int baseReward;
        public int taxAmount;
    }
}
