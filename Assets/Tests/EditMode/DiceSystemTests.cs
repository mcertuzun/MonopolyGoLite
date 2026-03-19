using NUnit.Framework;
using MonopolyLite.Logic;
using MonopolyLite.State;

namespace MonopolyLite.Tests
{
    public class DiceSystemTests
    {
        // 1. Roll_ReturnsDiceTotal_Between2And12
        [Test]
        public void Roll_ReturnsDiceTotal_Between2And12()
        {
            var dice = new DiceSystem(42);
            var player = new PlayerState(startingDice: 100, diceCap: 999);

            for (int i = 0; i < 50; i++)
            {
                var result = dice.Roll(player);
                Assert.IsTrue(result.Success);
                Assert.GreaterOrEqual(result.Total, 2);
                Assert.LessOrEqual(result.Total, 12);
                Assert.GreaterOrEqual(result.Die1, 1);
                Assert.LessOrEqual(result.Die1, 6);
                Assert.GreaterOrEqual(result.Die2, 1);
                Assert.LessOrEqual(result.Die2, 6);
                Assert.AreEqual(result.Die1 + result.Die2, result.Total);
            }
        }

        // 2. Roll_ConsumesDiceByMultiplier
        [Test]
        public void Roll_ConsumesDiceByMultiplier()
        {
            var dice = new DiceSystem(1);
            var player = new PlayerState(startingDice: 30, diceCap: 999);
            player.Multiplier = 3;

            var result = dice.Roll(player);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(27, player.Dice);
        }

        // 3. Roll_FailsWhenNotEnoughDice
        [Test]
        public void Roll_FailsWhenNotEnoughDice()
        {
            var dice = new DiceSystem(7);
            var player = new PlayerState(startingDice: 2, diceCap: 999);
            player.Multiplier = 5;

            var result = dice.Roll(player);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(2, player.Dice);
        }

        // 4. Roll_IsReproducibleWithSameSeed
        [Test]
        public void Roll_IsReproducibleWithSameSeed()
        {
            var diceA = new DiceSystem(999);
            var diceB = new DiceSystem(999);
            var playerA = new PlayerState(startingDice: 100, diceCap: 999);
            var playerB = new PlayerState(startingDice: 100, diceCap: 999);

            for (int i = 0; i < 10; i++)
            {
                var resultA = diceA.Roll(playerA);
                var resultB = diceB.Roll(playerB);
                Assert.AreEqual(resultA.Die1, resultB.Die1);
                Assert.AreEqual(resultA.Die2, resultB.Die2);
                Assert.AreEqual(resultA.Total, resultB.Total);
                Assert.AreEqual(resultA.IsDoubles, resultB.IsDoubles);
            }
        }

        // 5. Roll_DetectsDoubles
        [Test]
        public void Roll_DetectsDoubles()
        {
            // Seed chosen to guarantee at least one doubles in a reasonable run;
            // we verify the IsDoubles flag matches the actual dice values.
            var dice = new DiceSystem(0);
            var player = new PlayerState(startingDice: 500, diceCap: 999);

            bool foundDoubles = false;
            bool foundNonDoubles = false;

            for (int i = 0; i < 200; i++)
            {
                var result = dice.Roll(player);
                Assert.IsTrue(result.Success);
                bool expected = result.Die1 == result.Die2;
                Assert.AreEqual(expected, result.IsDoubles);

                if (result.IsDoubles) foundDoubles = true;
                else foundNonDoubles = true;
            }

            Assert.IsTrue(foundDoubles, "Expected at least one doubles result in 200 rolls");
            Assert.IsTrue(foundNonDoubles, "Expected at least one non-doubles result in 200 rolls");
        }
    }
}
