using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class MissionSystem
    {
        readonly MissionDef[] _pool;
        RNG _rng;

        public MissionSystem(MissionDef[] pool, int seed)
        {
            _pool = pool;
            _rng = new RNG((uint)seed);
        }

        public MissionProgress[] GenerateDaily(int count)
        {
            int actualCount = System.Math.Min(count, _pool.Length);
            var used = new bool[_pool.Length];
            var missions = new MissionProgress[actualCount];

            for (int i = 0; i < actualCount; i++)
            {
                int idx;
                do { idx = _rng.Next(0, _pool.Length); }
                while (used[idx]);

                used[idx] = true;
                var def = _pool[idx];
                missions[i] = new MissionProgress
                {
                    Type = def.type,
                    Description = string.Format(def.description, def.target),
                    Target = def.target,
                    Progress = 0,
                    CoinReward = def.coinReward,
                    DiceReward = def.diceReward,
                };
            }

            return missions;
        }

        public void TrackProgress(MissionProgress[] missions, MissionType type, int amount)
        {
            if (missions == null) return;
            foreach (var m in missions)
            {
                if (m.Type == type && !m.Completed)
                    m.Progress += amount;
            }
        }

        public bool AllCompleted(MissionProgress[] missions)
        {
            if (missions == null || missions.Length == 0) return false;
            foreach (var m in missions)
                if (!m.Completed) return false;
            return true;
        }
    }
}
