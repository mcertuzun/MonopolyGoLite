using MonopolyLite.Data;

namespace MonopolyLite.Logic
{
    public class ShutdownSystem
    {
        const int ShieldedReward = 50;
        const int UnshieldedReward = 500;

        public ShutdownResult Resolve(TargetProfile target, ColorGroup chosenLandmark,
                                      int multiplier, float boardMultiplier)
        {
            bool shielded = target.shields > 0;
            int baseReward = shielded ? ShieldedReward : UnshieldedReward;
            int coinsEarned = (int)(baseReward * multiplier * boardMultiplier);

            return new ShutdownResult
            {
                Shielded = shielded,
                CoinsEarned = coinsEarned,
                TargetedLandmark = chosenLandmark,
                TargetName = target.displayName,
            };
        }
    }
}
