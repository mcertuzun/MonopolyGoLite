using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct MilestoneDef
    {
        public int nwThreshold;        // Net Worth required to trigger
        public int diceCap;            // New dice cap (0 = no change)
        public int diceRegenSeconds;   // Seconds per dice regen (0 = no change)
        public int unlockedMultiplier; // New multiplier tier unlocked (0 = none)
    }
}
