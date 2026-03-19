using System.Collections.Generic;
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class MilestoneSystem
    {
        readonly MilestoneDef[] _milestones;

        public MilestoneSystem(MilestoneDef[] milestones)
        {
            _milestones = milestones;
        }

        public List<int> CheckAndApply(PlayerState player, ProgressionState progression)
        {
            var applied = new List<int>();

            for (int i = 0; i < _milestones.Length; i++)
            {
                if (progression.ClaimedMilestones.Contains(i))
                    continue;

                var m = _milestones[i];
                if (player.NetWorth < m.nwThreshold)
                    continue;

                progression.ClaimedMilestones.Add(i);
                applied.Add(i);

                if (m.diceCap > 0)
                    player.SetDiceCap(m.diceCap);

                if (m.diceRegenSeconds > 0)
                    progression.DiceRegenSeconds = m.diceRegenSeconds;

                if (m.unlockedMultiplier > 0 && !progression.IsMultiplierUnlocked(m.unlockedMultiplier))
                    progression.UnlockedMultipliers.Add(m.unlockedMultiplier);
            }

            return applied;
        }
    }
}
