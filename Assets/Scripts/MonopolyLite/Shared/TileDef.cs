using System;
using UnityEngine;

namespace MonopolyLite
{
    [Serializable]
    public struct TileDef
    {
        public string name;
        public TileType type;
        public int price;
        public int baseRent;
        public int taxAmount;
    }
}
