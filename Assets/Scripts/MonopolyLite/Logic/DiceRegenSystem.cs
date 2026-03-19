using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class DiceRegenSystem
    {
        const long TicksPerSecond = 10_000_000L;

        public int ApplyRegen(PlayerState player, ProgressionState progression, long currentTicks)
        {
            if (progression.LastRegenTicks == 0)
            {
                progression.LastRegenTicks = currentTicks;
                return 0;
            }

            long elapsed = currentTicks - progression.LastRegenTicks;
            long intervalTicks = (long)progression.DiceRegenSeconds * TicksPerSecond;
            int diceToGrant = (int)(elapsed / intervalTicks);

            if (diceToGrant <= 0)
                return 0;

            int space = player.DiceCap - player.Dice;
            int granted = System.Math.Min(diceToGrant, space);
            granted = System.Math.Max(granted, 0);

            if (granted > 0)
                player.AddDice(granted);

            // Advance by consumed intervals to preserve fractional time
            progression.LastRegenTicks += diceToGrant * intervalTicks;

            return granted;
        }
    }
}
