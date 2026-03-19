using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public struct MoveResult
    {
        public bool PassedGo;
        public int LandedTileIndex;
    }

    public class MovementSystem
    {
        public MoveResult Move(GameState state, int steps)
        {
            int boardSize = state.BoardDef.tiles.Length;
            int oldPos = state.Player.Position;
            int newPos = (oldPos + steps) % boardSize;
            bool passedGo = newPos < oldPos;

            state.Player.Position = newPos;

            if (passedGo)
            {
                int bonus = state.BoardDef.goBonus * state.Player.Multiplier;
                state.Player.AddCoins(bonus);
            }

            return new MoveResult { PassedGo = passedGo, LandedTileIndex = newPos };
        }

        public void MoveToTile(GameState state, int tileIndex, bool grantGoBonus)
        {
            int oldPos = state.Player.Position;
            bool passedGo = grantGoBonus && tileIndex < oldPos;

            state.Player.Position = tileIndex;

            if (passedGo)
            {
                int bonus = state.BoardDef.goBonus * state.Player.Multiplier;
                state.Player.AddCoins(bonus);
            }
        }
    }
}
