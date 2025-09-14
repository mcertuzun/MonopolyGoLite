using UnityEngine;

namespace MonopolyLite
{
    public enum GameMode
    {
        Simulation,
        Player
    }

    public class GameDriver : MonoBehaviour
    {
        public uint seed = 12345;
        public int ticksPerSecond = Deterministic.FixedHz;
        public GameMode mode = GameMode.Player;
        private MonopolyLiteGame _game;
        private float accum;
        public MonopolyLiteGame Game => _game;

        private void Update()
        {
            accum += Time.deltaTime;
            float step = 1f / ticksPerSecond;
            while (accum >= step)
            {
                accum -= step;
                if (mode == GameMode.Simulation)
                {
                    int p = _game.state.currentPlayer;
                    _game.Enqueue(Command.Roll(p));
                }

                _game.Tick();
                if (_game.Frame % 90 == 0) _game.ReplayAndVerify(out ulong f, out ulong r);
            }
        }

        public void Init(BoardConfig bc, LiveOpsConfig lc)
        {
            _game = new MonopolyLiteGame();
            _game.Init(bc, lc, seed);
        }
    }
}