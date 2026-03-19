using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public struct RollResult
    {
        public bool Success;
        public int Die1;
        public int Die2;
        public int Total;
        public bool IsDoubles;
    }

    public class DiceSystem
    {
        RNG _rng;

        public DiceSystem(int seed)
        {
            _rng = new RNG((uint)seed);
        }

        public RollResult Roll(PlayerState player)
        {
            if (!player.ConsumeDice())
                return new RollResult { Success = false };

            int die1 = _rng.Next(1, 7);
            int die2 = _rng.Next(1, 7);
            return new RollResult
            {
                Success = true,
                Die1 = die1,
                Die2 = die2,
                Total = die1 + die2,
                IsDoubles = die1 == die2
            };
        }
    }
}
