using System;

namespace MonopolyLite
{
    [Serializable]
    public class GameConfigJson
    {
        public float sideLength, tileSize;
        public TileDef[] tiles;
        public int startingCash, goPayout, jailTileIndex;
        public uint seed;
        public int ticksPerSecond, targetWidth, targetHeight;
        public float cameraMargin;
        public int initialCharges, chargeCap;
        public float chargeInterval;
    }
}
