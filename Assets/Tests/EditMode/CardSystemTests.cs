using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class CardSystemTests
    {
        BoardDef _board;
        GameState _state;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardDef
            {
                tiles = new TileDef[32],
                goTileIndex = 0,
                goBonus = 200,
                jailTileIndex = 8,
                landmarks = new LandmarkDef[0],
                chanceCards = new CardDef[]
                {
                    new CardDef { type = CardType.GainCoins,  amount = 100 },
                    new CardDef { type = CardType.LoseCoins,  amount = 100 },
                    new CardDef { type = CardType.GainDice,   amount = 30  },
                    new CardDef { type = CardType.GainShield, amount = 1   },
                    new CardDef { type = CardType.GoToJail                 },
                    new CardDef { type = CardType.GoToTile,   tileIndex = 10 },
                },
                communityChestCards = new CardDef[]
                {
                    new CardDef { type = CardType.GainCoins, amount = 50 },
                },
            };
            _state = new GameState(_board, startingDice: 100, diceCap: 1000);
        }

        // 1. DrawChance_ReturnsCardAndAdvancesIndex
        [Test]
        public void DrawChance_ReturnsCardAndAdvancesIndex()
        {
            var system = new CardSystem(seed: 0);

            int before = _state.Board.ChanceDrawIndex;
            var card = system.DrawChance(_state);
            int after = _state.Board.ChanceDrawIndex;

            // A card from the deck was returned
            Assert.IsNotNull(card.type.ToString());
            // Index advanced by exactly 1 (modulo deck size)
            Assert.AreEqual((before + 1) % _board.chanceCards.Length, after);
        }

        // 2. DrawChance_WrapsAroundDeck
        [Test]
        public void DrawChance_WrapsAroundDeck()
        {
            var system = new CardSystem(seed: 0);

            // Draw all 6 cards
            for (int i = 0; i < _board.chanceCards.Length; i++)
                system.DrawChance(_state);

            // Index must have wrapped back to 0
            Assert.AreEqual(0, _state.Board.ChanceDrawIndex);
        }

        // 3. ApplyCard_GainCoins_ScalesWithMultiplier
        [Test]
        public void ApplyCard_GainCoins_ScalesWithMultiplier()
        {
            var system = new CardSystem(seed: 0);
            _state.Player.Multiplier = 3;
            var card = new CardDef { type = CardType.GainCoins, amount = 100 };

            system.ApplyCard(_state, card);

            Assert.AreEqual(300, _state.Player.Coins);
        }

        // 4. ApplyCard_LoseCoins_DoesNotScaleWithMultiplier
        [Test]
        public void ApplyCard_LoseCoins_DoesNotScaleWithMultiplier()
        {
            var system = new CardSystem(seed: 0);
            _state.Player.Multiplier = 3;
            _state.Player.AddCoins(500); // Give enough coins to spend
            var card = new CardDef { type = CardType.LoseCoins, amount = 100 };

            system.ApplyCard(_state, card);

            // Flat 100 deducted regardless of multiplier
            Assert.AreEqual(400, _state.Player.Coins);
        }

        // 5. ApplyCard_GainDice_AddsDice
        [Test]
        public void ApplyCard_GainDice_AddsDice()
        {
            var system = new CardSystem(seed: 0);
            var card = new CardDef { type = CardType.GainDice, amount = 30 };

            system.ApplyCard(_state, card);

            // 100 starting + 30 = 130
            Assert.AreEqual(130, _state.Player.Dice);
        }

        // 6. ApplyCard_GainShield_AddsShield
        [Test]
        public void ApplyCard_GainShield_AddsShield()
        {
            var system = new CardSystem(seed: 0);
            var card = new CardDef { type = CardType.GainShield, amount = 1 };

            system.ApplyCard(_state, card);

            Assert.AreEqual(1, _state.Player.Shields);
        }

        // 7. ApplyCard_GoToJail_SetsJail
        [Test]
        public void ApplyCard_GoToJail_SetsJail()
        {
            var system = new CardSystem(seed: 0);
            var card = new CardDef { type = CardType.GoToJail };

            system.ApplyCard(_state, card);

            Assert.AreEqual(_board.jailTileIndex, _state.Player.Position);
            Assert.AreEqual(3, _state.Player.JailTurnsLeft);
        }

        // 8. ApplyCard_GoToTile_MovesPlayer
        [Test]
        public void ApplyCard_GoToTile_MovesPlayer()
        {
            var system = new CardSystem(seed: 0);
            var card = new CardDef { type = CardType.GoToTile, tileIndex = 10 };

            system.ApplyCard(_state, card);

            Assert.AreEqual(10, _state.Player.Position);
        }
    }
}
