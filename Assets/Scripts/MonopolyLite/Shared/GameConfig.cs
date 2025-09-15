using System;
using UnityEngine;

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

    [Serializable]
    public struct TileDef
    {
        public string name;
        public TileType type;
        public int price;
        public int baseRent;
        public int taxAmount;
    }

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

    public static class ConfigLoader
    {
        public static GameConfig LoadOrDefault()
        {
            GameConfig cfg = Resources.Load<GameConfig>("GameConfig");
            if (cfg != null) return cfg;
            TextAsset ta = Resources.Load<TextAsset>("gameconfig");
            if (ta == null) return BuildDefault();
            try
            {
                GameConfigJson j = JsonUtility.FromJson<GameConfigJson>(ta.text);
                GameConfig so = ScriptableObject.CreateInstance<GameConfig>();
                so.sideLength = j.sideLength;
                so.tileSize = j.tileSize;
                so.tiles = j.tiles;
                so.startingCash = j.startingCash;
                so.goPayout = j.goPayout;
                so.jailTileIndex = j.jailTileIndex;
                so.seed = j.seed;
                so.ticksPerSecond = j.ticksPerSecond;
                so.targetWidth = j.targetWidth;
                so.targetHeight = j.targetHeight;
                so.cameraMargin = j.cameraMargin;
                so.initialCharges = j.initialCharges;
                so.chargeCap = j.chargeCap;
                so.chargeInterval = j.chargeInterval;
                return so;
            }
            catch
            {
                return BuildDefault();
            }
        }

        private static GameConfig BuildDefault()
        {
            GameConfig so = ScriptableObject.CreateInstance<GameConfig>();
            so.tiles = new TileDef[]
            { new()
              { name = "GO", type = TileType.Go },
              new()
              { name = "Mediterranean Ave", type = TileType.Property, price = 60, baseRent = 2 },
              new()
              { name = "Community Chest", type = TileType.Chest },
              new()
              { name = "Baltic Ave", type = TileType.Property, price = 60, baseRent = 4 },
              new()
              { name = "Income Tax", type = TileType.Tax, taxAmount = 200 },
              new()
              { name = "Reading Railroad", type = TileType.Property, price = 200, baseRent = 25 },
              new()
              { name = "Jail", type = TileType.Jail },
              new()
              { name = "Oriental Ave", type = TileType.Property, price = 100, baseRent = 6 },
              new()
              { name = "Chance", type = TileType.Chest },
              new()
              { name = "Vermont Ave", type = TileType.Property, price = 100, baseRent = 6 },
              new()
              { name = "Connecticut Ave", type = TileType.Property, price = 120, baseRent = 8 },
              new()
              { name = "Go To Jail", type = TileType.GoToJail } };
            return so;
        }
    }
}