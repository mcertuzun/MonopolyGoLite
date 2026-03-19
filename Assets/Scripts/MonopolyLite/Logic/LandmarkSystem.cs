using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class LandmarkSystem
    {
        public int GetUpgradeCost(GameState state, ColorGroup group)
        {
            int currentLevel = state.Board.GetLandmarkLevel(group);
            if (currentLevel >= 5) return -1;
            var landmark = FindLandmark(state.BoardDef, group);
            if (landmark == null) return -1;
            return landmark.Value.costs[currentLevel];
        }

        public bool CanUpgrade(GameState state, ColorGroup group)
        {
            int cost = GetUpgradeCost(state, group);
            if (cost < 0) return false;
            return state.Player.Coins >= cost;
        }

        public bool Upgrade(GameState state, ColorGroup group)
        {
            if (!CanUpgrade(state, group)) return false;
            int currentLevel = state.Board.GetLandmarkLevel(group);
            var landmark = FindLandmark(state.BoardDef, group).Value;
            int cost = landmark.costs[currentLevel];
            int nw = landmark.nwPoints[currentLevel];
            state.Player.SpendCoins(cost);
            state.Board.SetLandmarkLevel(group, currentLevel + 1);
            state.Player.NetWorth += nw;
            return true;
        }

        public bool IsBoardComplete(GameState state) => state.Board.IsComplete();

        static LandmarkDef? FindLandmark(BoardDef board, ColorGroup group)
        {
            foreach (var lm in board.landmarks)
                if (lm.colorGroup == group) return lm;
            return null;
        }
    }
}
