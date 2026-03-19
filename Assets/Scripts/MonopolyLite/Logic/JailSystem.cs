using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class JailSystem
    {
        readonly int _diceCost;

        public JailSystem(int jailDiceCost)
        {
            _diceCost = jailDiceCost;
        }

        public void SendToJail(GameState state)
        {
            state.Player.Position = state.BoardDef.jailTileIndex;
            state.Player.JailTurnsLeft = 3;
        }

        public bool IsInJail(GameState state) => state.Player.JailTurnsLeft > 0;

        public void TickJailTurn(GameState state)
        {
            if (state.Player.JailTurnsLeft > 0)
                state.Player.JailTurnsLeft--;
        }

        public bool PayToExit(GameState state)
        {
            if (!state.Player.SpendDice(_diceCost)) return false;
            state.Player.JailTurnsLeft = 0;
            return true;
        }

        public bool TryExitOnDoubles(GameState state, bool isDoubles)
        {
            if (!isDoubles) return false;
            state.Player.JailTurnsLeft = 0;
            return true;
        }
    }
}
