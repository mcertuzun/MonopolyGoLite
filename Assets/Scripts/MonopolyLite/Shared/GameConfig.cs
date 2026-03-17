using System;
using UnityEngine;

namespace MonopolyLite
{
    public enum TileType
    {
        Go,
        Property,
        Tax,
        Chance,
        CommunityChest,
        GoToJail,
        Jail,
        Railroad,
        Utility
    }

    public enum ColorGroup
    {
        None,
        Brown,
        LightBlue,
        Pink,
        Orange,
        Red,
        Yellow,
        Green,
        Blue
    }

    public enum CardType
    {
        GainMoney,
        LoseMoney,
        GoToTile,
        GoToJail,
        RepairCosts,
        GainPerProperty
    }

    [Serializable]
    public struct CardDef
    {
        public CardType type;
        public string description;
        public int amount;
        public int tileIndex;
        public int perHouse;
        public int perHotel;
    }

    [Serializable]
    public struct TileDef
    {
        public string name;
        public TileType type;
        public int price;
        public int baseRent;
        public int taxAmount;
        public ColorGroup colorGroup;
        public int[] rentTable;
        public int houseCost;
        public int hotelCost;
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
        public int[] railroadRentTiers = { 25, 50, 100, 200 };
        public int[] utilityRentFactors = { 4, 10 };
        public CardDef[] chanceCards;
        public CardDef[] communityChestCards;
        public Color propertyColor = new(0.20f, 0.62f, 0.86f);
        public Color taxColor = new(0.86f, 0.31f, 0.31f);
        public Color chestColor = new(0.47f, 0.86f, 0.47f);
        public Color goColor = new(0.95f, 0.84f, 0.34f);
        public Color jailColor = new(0.75f, 0.55f, 0.35f);
        public Color gotoJailColor = new(0.60f, 0.35f, 0.75f);
        public Color railroadColor = new(0.50f, 0.50f, 0.50f);
        public Color utilityColor = new(0.85f, 0.75f, 0.35f);
        public Color chanceColor = new(0.95f, 0.55f, 0.20f);
        public Color communityChestColor = new(0.30f, 0.70f, 0.90f);
        public Color brownGroup = new(0.55f, 0.33f, 0.16f);
        public Color lightBlueGroup = new(0.60f, 0.82f, 0.95f);
        public Color pinkGroup = new(0.85f, 0.30f, 0.65f);
        public Color orangeGroup = new(0.95f, 0.60f, 0.15f);
        public Color redGroup = new(0.90f, 0.20f, 0.20f);
        public Color yellowGroup = new(0.95f, 0.90f, 0.25f);
        public Color greenGroup = new(0.15f, 0.70f, 0.30f);
        public Color blueGroup = new(0.10f, 0.20f, 0.70f);
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
        public int[] railroadRentTiers;
        public int[] utilityRentFactors;
        public CardDef[] chanceCards;
        public CardDef[] communityChestCards;
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
                if (j.railroadRentTiers != null) so.railroadRentTiers = j.railroadRentTiers;
                if (j.utilityRentFactors != null) so.utilityRentFactors = j.utilityRentFactors;
                so.chanceCards = j.chanceCards;
                so.communityChestCards = j.communityChestCards;
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
              { name = "Mediterranean Ave", type = TileType.Property, price = 60, colorGroup = ColorGroup.Brown, rentTable = new[]{ 2, 10, 30, 90, 160, 250 }, houseCost = 50, hotelCost = 50 },
              new()
              { name = "Community Chest", type = TileType.CommunityChest },
              new()
              { name = "Baltic Ave", type = TileType.Property, price = 60, colorGroup = ColorGroup.Brown, rentTable = new[]{ 4, 20, 60, 180, 320, 450 }, houseCost = 50, hotelCost = 50 },
              new()
              { name = "Income Tax", type = TileType.Tax, taxAmount = 200 },
              new()
              { name = "Reading Railroad", type = TileType.Railroad, price = 200 },
              new()
              { name = "Jail", type = TileType.Jail },
              new()
              { name = "Oriental Ave", type = TileType.Property, price = 100, colorGroup = ColorGroup.LightBlue, rentTable = new[]{ 6, 30, 90, 270, 400, 550 }, houseCost = 50, hotelCost = 50 },
              new()
              { name = "Chance", type = TileType.Chance },
              new()
              { name = "Vermont Ave", type = TileType.Property, price = 100, colorGroup = ColorGroup.LightBlue, rentTable = new[]{ 6, 30, 90, 270, 400, 550 }, houseCost = 50, hotelCost = 50 },
              new()
              { name = "Connecticut Ave", type = TileType.Property, price = 120, colorGroup = ColorGroup.LightBlue, rentTable = new[]{ 8, 40, 100, 300, 450, 600 }, houseCost = 50, hotelCost = 50 },
              new()
              { name = "Go To Jail", type = TileType.GoToJail } };
            so.chanceCards = DefaultChanceCards();
            so.communityChestCards = DefaultCommunityChestCards();
            return so;
        }

        private static CardDef[] DefaultChanceCards()
        {
            return new CardDef[]
            {
                new() { type = CardType.GainMoney, description = "Bank pays you dividend of $50", amount = 50 },
                new() { type = CardType.GoToTile, description = "Advance to GO", tileIndex = 0 },
                new() { type = CardType.GoToJail, description = "Go directly to Jail" },
                new() { type = CardType.LoseMoney, description = "Speeding fine $15", amount = 15 },
                new() { type = CardType.GainMoney, description = "Building loan matures — collect $150", amount = 150 },
                new() { type = CardType.LoseMoney, description = "Pay poor tax of $15", amount = 15 },
                new() { type = CardType.RepairCosts, description = "Make general repairs: $25/house, $100/hotel", perHouse = 25, perHotel = 100 },
                new() { type = CardType.GainPerProperty, description = "Collect $50 per property owned", amount = 50 },
                new() { type = CardType.GainMoney, description = "Your investment matures — collect $100", amount = 100 },
                new() { type = CardType.LoseMoney, description = "Pay hospital fees of $100", amount = 100 }
            };
        }

        private static CardDef[] DefaultCommunityChestCards()
        {
            return new CardDef[]
            {
                new() { type = CardType.GainMoney, description = "Bank error in your favor — collect $200", amount = 200 },
                new() { type = CardType.LoseMoney, description = "Doctor's fees — pay $50", amount = 50 },
                new() { type = CardType.GainMoney, description = "From sale of stock you get $50", amount = 50 },
                new() { type = CardType.GoToJail, description = "Go to Jail" },
                new() { type = CardType.GainMoney, description = "Holiday fund matures — receive $100", amount = 100 },
                new() { type = CardType.GainMoney, description = "Income tax refund — collect $20", amount = 20 },
                new() { type = CardType.GainMoney, description = "Life insurance matures — collect $100", amount = 100 },
                new() { type = CardType.LoseMoney, description = "Pay school fees of $50", amount = 50 },
                new() { type = CardType.GainMoney, description = "Receive $25 consultancy fee", amount = 25 },
                new() { type = CardType.RepairCosts, description = "Street repairs: $40/house, $115/hotel", perHouse = 40, perHotel = 115 },
                new() { type = CardType.GainMoney, description = "Beauty contest — collect $10", amount = 10 },
                new() { type = CardType.GainMoney, description = "You inherit $100", amount = 100 }
            };
        }
    }
}