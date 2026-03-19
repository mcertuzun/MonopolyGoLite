using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class DailyLoginSystem
    {
        readonly DailyRewardDef[] _rewards;

        public DailyLoginSystem(DailyRewardDef[] rewards)
        {
            _rewards = rewards;
        }

        public bool CanClaim(ProgressionState progression, string today)
        {
            return progression.LastLoginDate != today;
        }

        public DailyRewardDef? Claim(PlayerState player, ProgressionState progression, string today)
        {
            if (!CanClaim(progression, today))
                return null;

            if (IsConsecutiveDay(progression.LastLoginDate, today))
            {
                progression.LoginStreak++;
                if (progression.LoginStreak > _rewards.Length)
                    progression.LoginStreak = 1;
            }
            else
            {
                progression.LoginStreak = 1;
            }

            progression.LastLoginDate = today;

            int dayIndex = progression.LoginStreak - 1;
            var reward = _rewards[dayIndex];

            player.AddCoins(reward.coins);
            player.AddDice(reward.dice);

            return reward;
        }

        static bool IsConsecutiveDay(string lastDate, string today)
        {
            if (string.IsNullOrEmpty(lastDate))
                return false;

            if (!System.DateTime.TryParseExact(lastDate, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var last))
                return false;

            if (!System.DateTime.TryParseExact(today, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var current))
                return false;

            return (current - last).Days == 1;
        }
    }
}
