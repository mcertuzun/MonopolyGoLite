using MonopolyLite.Data;

namespace MonopolyLite.Config
{
    public static class StickerConfigLoader
    {
        public static AlbumDef CreateDefault()
        {
            return new AlbumDef
            {
                name = "Istanbul Collection",
                sets = new StickerSetDef[]
                {
                    new StickerSetDef { name = "Hagia Sophia",  stickerCount = 6, coinReward = 1000, diceReward = 50 },
                    new StickerSetDef { name = "Grand Bazaar",  stickerCount = 6, coinReward = 1500, diceReward = 75 },
                    new StickerSetDef { name = "Bosphorus",     stickerCount = 6, coinReward = 2000, diceReward = 100 },
                    new StickerSetDef { name = "Galata Tower",  stickerCount = 6, coinReward = 3000, diceReward = 150 },
                },
                stickers = BuildStickers(),
            };
        }

        static StickerDef[] BuildStickers()
        {
            var stickers = new StickerDef[24];
            string[][] names =
            {
                new[] { "Dome", "Minaret", "Fountain", "Garden", "Interior", "Mosaic" },
                new[] { "Carpet", "Lamp", "Spice", "Tea Set", "Jewelry", "Ceramic" },
                new[] { "Ferry", "Bridge", "Sunset", "Fisherman", "Seagull", "Lighthouse" },
                new[] { "Tower", "View", "Stairs", "Museum", "Cafe", "Night" },
            };
            StickerRarity[][] rarities =
            {
                new[] { StickerRarity.Star1, StickerRarity.Star1, StickerRarity.Star2, StickerRarity.Star2, StickerRarity.Star3, StickerRarity.Star4 },
                new[] { StickerRarity.Star1, StickerRarity.Star1, StickerRarity.Star2, StickerRarity.Star3, StickerRarity.Star3, StickerRarity.Star5 },
                new[] { StickerRarity.Star1, StickerRarity.Star2, StickerRarity.Star2, StickerRarity.Star3, StickerRarity.Star4, StickerRarity.Star5 },
                new[] { StickerRarity.Star1, StickerRarity.Star1, StickerRarity.Star2, StickerRarity.Star3, StickerRarity.Star4, StickerRarity.Star5 },
            };

            int id = 0;
            for (int set = 0; set < 4; set++)
            {
                for (int s = 0; s < 6; s++)
                {
                    stickers[id] = new StickerDef
                    {
                        id = id,
                        name = names[set][s],
                        setIndex = set,
                        rarity = rarities[set][s],
                    };
                    id++;
                }
            }
            return stickers;
        }
    }
}
