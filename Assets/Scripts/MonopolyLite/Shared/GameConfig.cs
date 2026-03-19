using UnityEngine;

namespace MonopolyLite
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MonopolyLite/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public float sideLength = 12f;
        public float tileSize = 1.8f;
        public TileDef[] tiles;
        public int startingCash = 1500;
        public int goPayout = 200;
        public int jailTileIndex = 6;
        public uint seed = 12345;
        public int ticksPerSecond = 30;
        public int targetWidth = 1080;
        public int targetHeight = 1920;
        public float cameraMargin = 1f;
        public int initialCharges = 3;
        public int chargeCap = 20;
        public float chargeInterval = 3f;
        public Color propertyColor = new(0.20f, 0.62f, 0.86f);
        public Color taxColor = new(0.86f, 0.31f, 0.31f);
        public Color chestColor = new(0.47f, 0.86f, 0.47f);
        public Color goColor = new(0.95f, 0.84f, 0.34f);
        public Color jailColor = new(0.75f, 0.55f, 0.35f);
        public Color gotoJailColor = new(0.60f, 0.35f, 0.75f);
        public Color tokenA = new(0.2f, 0.6f, 1f);
        public Color tokenB = new(1f, 0.5f, 0.2f);
        public Color rollColor = new(0.15f, 0.8f, 0.35f);
        public Color rollDisabled = new(0.35f, 0.35f, 0.35f);
        public Color multColor = new(0.15f, 0.35f, 0.9f);
    }
}
