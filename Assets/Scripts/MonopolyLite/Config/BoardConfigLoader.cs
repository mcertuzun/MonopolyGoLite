using MonopolyLite.Data;
using UnityEngine;

namespace MonopolyLite.Config
{
    public static class BoardConfigLoader
    {
        /// <summary>
        /// Loads a BoardDef from Resources/Boards/{boardId}.json.
        /// Falls back to CreateDefault() if the asset is not found or fails to parse.
        /// </summary>
        public static BoardDef Load(string boardId)
        {
            string path = $"Boards/{boardId}";
            TextAsset asset = Resources.Load<TextAsset>(path);
            if (asset != null)
            {
                try
                {
                    BoardDef loaded = JsonUtility.FromJson<BoardDef>(asset.text);
                    if (loaded != null && loaded.tiles != null && loaded.tiles.Length > 0)
                        return loaded;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[BoardConfigLoader] Failed to parse '{path}': {e.Message}. Using default.");
                }
            }
            else
            {
                Debug.Log($"[BoardConfigLoader] '{path}' not found in Resources. Using default Istanbul board.");
            }

            return CreateDefault();
        }

        /// <summary>
        /// Returns the default Istanbul-themed 32-tile board.
        /// </summary>
        public static BoardDef CreateDefault()
        {
            return new BoardDef
            {
                id             = "istanbul",
                theme          = "Istanbul",
                sideLength     = 16f,
                tileSize       = 1.4f,
                jailTileIndex  = 8,
                goTileIndex    = 0,
                goBonus        = 200,
                boardMultiplier = 1.0f,
                tiles          = BuildTiles(),
                landmarks      = BuildLandmarks(),
                chanceCards    = BuildChanceCards(),
                communityChestCards = BuildCommunityChestCards()
            };
        }

        // ── Tiles ────────────────────────────────────────────────────────────

        private static TileDef[] BuildTiles()
        {
            return new TileDef[]
            {
                // 0 — GO
                T("GO",              TileType.Go,            ColorGroup.None,      0,   0),
                // 1,3 — Brown
                T("Sultanahmet",     TileType.Property,      ColorGroup.Brown,    50,   0),
                // 2 — Community Chest
                T("Community Chest", TileType.CommunityChest, ColorGroup.None,    0,   0),
                // 3 — Brown
                T("Balat",           TileType.Property,      ColorGroup.Brown,    60,   0),
                // 4 — Income Tax
                T("Income Tax",      TileType.Tax,           ColorGroup.None,      0, 150),
                // 5 — Railroad
                T("Haydarpasa RR",   TileType.Railroad,      ColorGroup.None,    100,   0),
                // 6 — LightBlue
                T("Kadikoy",         TileType.Property,      ColorGroup.LightBlue, 80,  0),
                // 7 — Chance
                T("Chance",          TileType.Chance,        ColorGroup.None,      0,   0),
                // 8 — Jail
                T("Jail",            TileType.Jail,          ColorGroup.None,      0,   0),
                // 9 — LightBlue
                T("Moda",            TileType.Property,      ColorGroup.LightBlue, 90,  0),
                // 10 — Pink
                T("Besiktas",        TileType.Property,      ColorGroup.Pink,    100,   0),
                // 11 — Community Chest
                T("Community Chest", TileType.CommunityChest, ColorGroup.None,    0,   0),
                // 12 — Pink
                T("Ortakoy",         TileType.Property,      ColorGroup.Pink,    110,   0),
                // 13 — Railroad
                T("Sirkeci RR",      TileType.Railroad,      ColorGroup.None,    100,   0),
                // 14 — Orange
                T("Bebek",           TileType.Property,      ColorGroup.Orange,  120,   0),
                // 15 — Chance
                T("Chance",          TileType.Chance,        ColorGroup.None,      0,   0),
                // 16 — Free Parking
                T("Free Parking",    TileType.FreeParking,   ColorGroup.None,      0,   0),
                // 17 — Orange
                T("Nisantasi",       TileType.Property,      ColorGroup.Orange,  130,   0),
                // 18 — Red
                T("Etiler",          TileType.Property,      ColorGroup.Red,     140,   0),
                // 19 — Community Chest
                T("Community Chest", TileType.CommunityChest, ColorGroup.None,    0,   0),
                // 20 — Red
                T("Levent",          TileType.Property,      ColorGroup.Red,     150,   0),
                // 21 — Railroad
                T("Eminonu RR",      TileType.Railroad,      ColorGroup.None,    100,   0),
                // 22 — Yellow
                T("Taksim",          TileType.Property,      ColorGroup.Yellow,  160,   0),
                // 23 — Chance
                T("Chance",          TileType.Chance,        ColorGroup.None,      0,   0),
                // 24 — Go To Jail
                T("Go To Jail",      TileType.GoToJail,      ColorGroup.None,      0,   0),
                // 25 — Yellow
                T("Istiklal",        TileType.Property,      ColorGroup.Yellow,  170,   0),
                // 26 — Green
                T("Galata",          TileType.Property,      ColorGroup.Green,   180,   0),
                // 27 — Luxury Tax
                T("Luxury Tax",      TileType.Tax,           ColorGroup.None,      0, 200),
                // 28 — Green
                T("Karakoy",         TileType.Property,      ColorGroup.Green,   190,   0),
                // 29 — Railroad
                T("Kabatas RR",      TileType.Railroad,      ColorGroup.None,    100,   0),
                // 30 — Blue
                T("Uskudar",         TileType.Property,      ColorGroup.Blue,    200,   0),
                // 31 — Blue
                T("Beylerbeyi",      TileType.Property,      ColorGroup.Blue,    210,   0),
            };
        }

        private static TileDef T(string name, TileType type, ColorGroup cg, int reward, int tax)
        {
            return new TileDef
            {
                name        = name,
                type        = type,
                colorGroup  = cg,
                baseReward  = reward,
                taxAmount   = tax
            };
        }

        // ── Landmarks ────────────────────────────────────────────────────────

        private static LandmarkDef[] BuildLandmarks()
        {
            return new LandmarkDef[]
            {
                // Brown — Hagia Sophia
                new LandmarkDef
                {
                    colorGroup = ColorGroup.Brown,
                    name       = "Hagia Sophia",
                    costs      = new[] { 100, 200, 300, 400, 500 },
                    nwPoints   = new[] {  50, 120, 200, 290, 400 }
                },
                // LightBlue — Maiden's Tower
                new LandmarkDef
                {
                    colorGroup = ColorGroup.LightBlue,
                    name       = "Maiden's Tower",
                    costs      = new[] { 150, 300, 450, 600, 750 },
                    nwPoints   = new[] {  75, 180, 300, 440, 600 }
                },
                // Pink — Dolmabahce Palace
                new LandmarkDef
                {
                    colorGroup = ColorGroup.Pink,
                    name       = "Dolmabahce Palace",
                    costs      = new[] { 200, 400, 600, 800, 1000 },
                    nwPoints   = new[] { 100, 240, 400, 580,  800 }
                },
                // Orange — Topkapi Palace
                new LandmarkDef
                {
                    colorGroup = ColorGroup.Orange,
                    name       = "Topkapi Palace",
                    costs      = new[] { 250, 500, 750, 1000, 1250 },
                    nwPoints   = new[] { 125, 300, 500,  730, 1000 }
                },
                // Red — Blue Mosque
                new LandmarkDef
                {
                    colorGroup = ColorGroup.Red,
                    name       = "Blue Mosque",
                    costs      = new[] { 300, 600,  900, 1200, 1500 },
                    nwPoints   = new[] { 150, 360,  600,  870, 1200 }
                },
                // Yellow — Galata Tower
                new LandmarkDef
                {
                    colorGroup = ColorGroup.Yellow,
                    name       = "Galata Tower",
                    costs      = new[] { 350,  700, 1050, 1400, 1750 },
                    nwPoints   = new[] { 175,  420,  700, 1015, 1400 }
                },
                // Green — Grand Bazaar
                new LandmarkDef
                {
                    colorGroup = ColorGroup.Green,
                    name       = "Grand Bazaar",
                    costs      = new[] { 400,  800, 1200, 1600, 2000 },
                    nwPoints   = new[] { 200,  480,  800, 1160, 1600 }
                },
                // Blue — Bosphorus Bridge
                new LandmarkDef
                {
                    colorGroup = ColorGroup.Blue,
                    name       = "Bosphorus Bridge",
                    costs      = new[] { 500, 1000, 1500, 2000, 2500 },
                    nwPoints   = new[] { 250,  600, 1000, 1450, 2000 }
                },
            };
        }

        // ── Chance Cards ─────────────────────────────────────────────────────

        private static CardDef[] BuildChanceCards()
        {
            return new CardDef[]
            {
                C(CardType.GainCoins,  "A tourist tips you generously near the Grand Bazaar.",    150, -1),
                C(CardType.GainCoins,  "You sell a carpet at the Covered Bazaar for a profit.",   200, -1),
                C(CardType.LoseCoins,  "A seagull steals your simit on the Bosphorus ferry.",     100, -1),
                C(CardType.LoseCoins,  "Your taxi meter ran while stuck in Istanbul traffic.",     120, -1),
                C(CardType.GoToTile,   "Take the Metrobus to Taksim — advance to Taksim.",          0, 22),
                C(CardType.GoToTile,   "Hop a ferry to Kadikoy — move to Kadikoy.",                 0,  6),
                C(CardType.GoToJail,   "You were caught haggling illegally. Go to Jail.",            0, -1),
                C(CardType.GainDice,   "The Grand Bazaar merchant gives you a lucky die.",           1, -1),
                C(CardType.GainShield, "The Evil Eye amulet protects your next landing.",            1, -1),
                C(CardType.GainCoins,  "You win a backgammon tournament at a tea house.",          300, -1),
            };
        }

        // ── Community Chest Cards ────────────────────────────────────────────

        private static CardDef[] BuildCommunityChestCards()
        {
            return new CardDef[]
            {
                C(CardType.GainCoins,  "Istanbul municipality pays your property grant.",         200, -1),
                C(CardType.GainCoins,  "Your baklava shop earns record holiday sales.",           150, -1),
                C(CardType.LoseCoins,  "Pay your water bill to ISKI.",                            100, -1),
                C(CardType.LoseCoins,  "Earthquake insurance premium due.",                       130, -1),
                C(CardType.GoToTile,   "Return to GO — collect your bonus.",                        0,  0),
                C(CardType.GainCoins,  "Bank error in your favour near Galata.",                  250, -1),
                C(CardType.GainDice,   "The community sponsors an extra dice roll.",                1, -1),
                C(CardType.GainShield, "Neighbourly solidarity shields your next attack.",          1, -1),
                C(CardType.GainCoins,  "You inherit a small apartment in Beylerbeyi.",            180, -1),
                C(CardType.LoseCoins,  "Pay road construction tax on the Bosphorus Bridge.",      160, -1),
            };
        }

        private static CardDef C(CardType type, string description, int amount, int tileIndex)
        {
            return new CardDef
            {
                type        = type,
                description = description,
                amount      = amount,
                tileIndex   = tileIndex
            };
        }
    }
}
