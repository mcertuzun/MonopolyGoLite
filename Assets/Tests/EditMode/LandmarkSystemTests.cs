using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class LandmarkSystemTests
    {
        BoardDef _board;
        GameState _state;
        LandmarkSystem _system;

        // Landmarks:
        // Brown "Hagia Sophia":     costs=[500,1200,3000,7000,15000], nwPoints=[100,300,600,1200,2500]
        // Blue  "Bosphorus Bridge": costs=[1500,3000,7000,16000,38000], nwPoints=[250,650,1300,2600,5200]
        [SetUp]
        public void SetUp()
        {
            _board = new BoardDef
            {
                tiles = new TileDef[9],
                jailTileIndex = 6,
                goTileIndex = 0,
                goBonus = 200,
                chanceCards = new CardDef[0],
                communityChestCards = new CardDef[0],
                landmarks = new LandmarkDef[]
                {
                    new LandmarkDef
                    {
                        colorGroup = ColorGroup.Brown,
                        name = "Hagia Sophia",
                        costs    = new int[] { 500, 1200, 3000,  7000, 15000 },
                        nwPoints = new int[] { 100,  300,  600,  1200,  2500 },
                    },
                    new LandmarkDef
                    {
                        colorGroup = ColorGroup.Blue,
                        name = "Bosphorus Bridge",
                        costs    = new int[] { 1500, 3000,  7000, 16000, 38000 },
                        nwPoints = new int[] {  250,  650,  1300,  2600,  5200 },
                    },
                },
            };

            _state = new GameState(_board, startingDice: 100, diceCap: 1000);
            _system = new LandmarkSystem();
        }

        // 1. CanUpgrade returns true when player has enough coins at level 0
        [Test]
        public void CanUpgrade_TrueWhenEnoughCoins()
        {
            _state.Player.AddCoins(500);

            Assert.IsTrue(_system.CanUpgrade(_state, ColorGroup.Brown));
        }

        // 2. CanUpgrade returns false when player does not have enough coins
        [Test]
        public void CanUpgrade_FalseWhenNotEnoughCoins()
        {
            _state.Player.AddCoins(100);

            Assert.IsFalse(_system.CanUpgrade(_state, ColorGroup.Brown));
        }

        // 3. CanUpgrade returns false when landmark is already at max level (5)
        [Test]
        public void CanUpgrade_FalseWhenMaxLevel()
        {
            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 5);
            _state.Player.AddCoins(99999);

            Assert.IsFalse(_system.CanUpgrade(_state, ColorGroup.Brown));
        }

        // 4. Upgrade deducts correct coins and increases level by 1
        [Test]
        public void Upgrade_DeductsCoinsAndIncreasesLevel()
        {
            _state.Player.AddCoins(1000);

            bool result = _system.Upgrade(_state, ColorGroup.Brown);

            Assert.IsTrue(result);
            Assert.AreEqual(500, _state.Player.Coins);
            Assert.AreEqual(1, _state.Board.GetLandmarkLevel(ColorGroup.Brown));
        }

        // 5. Upgrade grants correct net worth for first upgrade (L0 -> L1)
        [Test]
        public void Upgrade_GrantsNetWorth()
        {
            _state.Player.AddCoins(1000);

            _system.Upgrade(_state, ColorGroup.Brown);

            Assert.AreEqual(100, _state.Player.NetWorth);
        }

        // 6. Two consecutive upgrades accumulate costs and net worth correctly
        [Test]
        public void Upgrade_SecondLevel_CostsMore()
        {
            _state.Player.AddCoins(2000);

            _system.Upgrade(_state, ColorGroup.Brown); // L0->L1: cost 500, nw 100
            _system.Upgrade(_state, ColorGroup.Brown); // L1->L2: cost 1200, nw 300

            Assert.AreEqual(300, _state.Player.Coins);
            Assert.AreEqual(2, _state.Board.GetLandmarkLevel(ColorGroup.Brown));
            Assert.AreEqual(400, _state.Player.NetWorth);
        }

        // 7. IsBoardComplete returns false when not all landmarks are at max level
        [Test]
        public void IsBoardComplete_FalseWhenNotAllMax()
        {
            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 5);
            // Blue is still at 0

            Assert.IsFalse(_system.IsBoardComplete(_state));
        }

        // 8. IsBoardComplete returns true when all landmarks are at max level (5)
        [Test]
        public void IsBoardComplete_TrueWhenAllMax()
        {
            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 5);
            _state.Board.SetLandmarkLevel(ColorGroup.Blue, 5);

            Assert.IsTrue(_system.IsBoardComplete(_state));
        }

        // 9. GetUpgradeCost returns the correct cost for each level
        [Test]
        public void GetUpgradeCost_ReturnsCorrectCostPerLevel()
        {
            // Level 0 -> cost 500
            Assert.AreEqual(500, _system.GetUpgradeCost(_state, ColorGroup.Brown));

            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 1);
            Assert.AreEqual(1200, _system.GetUpgradeCost(_state, ColorGroup.Brown));

            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 4);
            Assert.AreEqual(15000, _system.GetUpgradeCost(_state, ColorGroup.Brown));
        }

        // 10. GetUpgradeCost returns -1 when landmark is at max level
        [Test]
        public void GetUpgradeCost_ReturnsNegativeOneWhenMaxLevel()
        {
            _state.Board.SetLandmarkLevel(ColorGroup.Brown, 5);

            Assert.AreEqual(-1, _system.GetUpgradeCost(_state, ColorGroup.Brown));
        }
    }
}
