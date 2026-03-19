using UnityEngine;

namespace MonopolyLite
{
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
