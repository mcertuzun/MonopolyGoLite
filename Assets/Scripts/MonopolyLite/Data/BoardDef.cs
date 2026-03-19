using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public class BoardDef
    {
        public string id;
        public string theme;
        public float sideLength;
        public float tileSize;
        public int jailTileIndex;
        public int goTileIndex;
        public int goBonus;
        public float boardMultiplier;
        public TileDef[] tiles;
        public LandmarkDef[] landmarks;
        public CardDef[] chanceCards;
        public CardDef[] communityChestCards;
    }
}
