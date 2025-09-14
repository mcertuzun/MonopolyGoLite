using System;

namespace MonopolyLite
{
    public enum TileType
    {
        Go,
        Property,
        Tax,
        Chest,
        GoToJail,
        Jail
    }

    public struct Tile
    {
        public string name;
        public TileType type;
        public int price;
        public int baseRent;
        public int taxAmount;
    }

    [Serializable]
    public class BoardConfig
    {
        public int startingCash = 1500;
        public int goPayout = 200;
        public int jailTileIndex = 6;
        public Tile[] tiles;
    }
}