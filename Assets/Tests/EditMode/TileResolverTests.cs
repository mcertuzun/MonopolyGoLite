using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class TileResolverTests
    {
        BoardDef _board;
        GameState _state;
        CardSystem _cardSystem;
        JailSystem _jailSystem;
        TileResolver _resolver;

        // Board layout:
        // [0] GO, [1] Property Brown baseReward=50, [2] Tax taxAmount=150,
        // [3] Chance, [4] CommunityChest, [5] Railroad baseReward=100,
        // [6] Jail, [7] FreeParking, [8] GoToJail
        [SetUp]
        public void SetUp()
        {
            _board = new BoardDef
            {
                jailTileIndex = 6,
                goTileIndex = 0,
                goBonus = 200,
                tiles = new TileDef[]
                {
                    new TileDef { name = "Go",            type = TileType.Go           },
                    new TileDef { name = "Property",      type = TileType.Property,      colorGroup = ColorGroup.Brown, baseReward = 50  },
                    new TileDef { name = "Tax",           type = TileType.Tax,           taxAmount = 150                                  },
                    new TileDef { name = "Chance",        type = TileType.Chance                                                         },
                    new TileDef { name = "CommunityChest",type = TileType.CommunityChest                                                 },
                    new TileDef { name = "Railroad",      type = TileType.Railroad,      baseReward = 100                                 },
                    new TileDef { name = "Jail",          type = TileType.Jail                                                           },
                    new TileDef { name = "FreeParking",   type = TileType.FreeParking                                                    },
                    new TileDef { name = "GoToJail",      type = TileType.GoToJail                                                       },
                },
                landmarks = new LandmarkDef[0],
                chanceCards = new CardDef[]
                {
                    new CardDef { type = CardType.GainCoins, amount = 100 },
                },
                communityChestCards = new CardDef[]
                {
                    new CardDef { type = CardType.GainCoins, amount = 80 },
                },
            };

            _state = new GameState(_board, startingDice: 100, diceCap: 1000);
            _cardSystem = new CardSystem(seed: 0);
            _jailSystem = new JailSystem(jailDiceCost: 50);
            _resolver = new TileResolver(_cardSystem, _jailSystem);
        }

        // 1. Property tile grants coins scaled by multiplier
        [Test]
        public void Resolve_Property_GrantsCoinsTimesMultiplier()
        {
            _state.Player.Position = 1;
            _state.Player.Multiplier = 2;

            var result = _resolver.Resolve(_state);

            Assert.AreEqual(TileResolveType.CoinsGained, result.Type);
            Assert.AreEqual(100, result.Amount);
            Assert.AreEqual(100, _state.Player.Coins);
        }

        // 2. Tax tile deducts coins scaled by multiplier
        [Test]
        public void Resolve_Tax_LosesCoins()
        {
            _state.Player.Position = 2;
            _state.Player.Multiplier = 2;
            _state.Player.AddCoins(500);

            var result = _resolver.Resolve(_state);

            Assert.AreEqual(TileResolveType.CoinsLost, result.Type);
            Assert.AreEqual(300, result.Amount);
            Assert.AreEqual(200, _state.Player.Coins);
        }

        // 2b. Tax tile floors at zero when player can't afford it
        [Test]
        public void Resolve_Tax_InsufficientCoins_FloorsAtZero()
        {
            _state.Player.Position = 2;
            _state.Player.Multiplier = 2;
            _state.Player.AddCoins(100); // tax = 150*2 = 300, but only 100 available

            var result = _resolver.Resolve(_state);

            Assert.AreEqual(TileResolveType.CoinsLost, result.Type);
            Assert.AreEqual(100, result.Amount);   // actual deducted = 100
            Assert.AreEqual(0, _state.Player.Coins); // floors at zero
        }

        // 3. Railroad tile returns Railroad type with no coin grant (Heist/Shutdown delegated to GameController)
        [Test]
        public void Railroad_ReturnsRailroadType_NoCoinGrant()
        {
            _state.Player.Position = 5;
            _state.Player.Multiplier = 1;

            var result = _resolver.Resolve(_state);

            Assert.AreEqual(TileResolveType.Railroad, result.Type);
            Assert.AreEqual(0, result.Amount);
            Assert.AreEqual(0, _state.Player.Coins);
        }

        // 4. GoToJail tile sends player to jail tile and sets JailTurnsLeft=3
        [Test]
        public void Resolve_GoToJail_SendsToJail()
        {
            _state.Player.Position = 8;

            var result = _resolver.Resolve(_state);

            Assert.AreEqual(TileResolveType.Jail, result.Type);
            Assert.AreEqual(6, _state.Player.Position);
            Assert.AreEqual(3, _state.Player.JailTurnsLeft);
        }

        // 5. FreeParking tile has no effect
        [Test]
        public void Resolve_FreeParking_NoEffect()
        {
            _state.Player.Position = 7;

            var result = _resolver.Resolve(_state);

            Assert.AreEqual(TileResolveType.Nothing, result.Type);
            Assert.AreEqual(0, _state.Player.Coins);
        }

        // 6. Chance tile draws a card and applies it (GainCoins 100)
        [Test]
        public void Resolve_Chance_DrawsCard()
        {
            _state.Player.Position = 3;
            _state.Player.Multiplier = 1;

            var result = _resolver.Resolve(_state);

            Assert.AreEqual(TileResolveType.Card, result.Type);
            Assert.IsNotNull(result.DrawnCard);
            Assert.AreEqual(100, _state.Player.Coins);
        }

        // 7. CommunityChest tile draws a card and applies it (GainCoins 80)
        [Test]
        public void Resolve_CommunityChest_DrawsCard()
        {
            _state.Player.Position = 4;
            _state.Player.Multiplier = 1;

            var result = _resolver.Resolve(_state);

            Assert.AreEqual(TileResolveType.Card, result.Type);
            Assert.IsNotNull(result.DrawnCard);
            Assert.AreEqual(80, _state.Player.Coins);
        }
    }
}
