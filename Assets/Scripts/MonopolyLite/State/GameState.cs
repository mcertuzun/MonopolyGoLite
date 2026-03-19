using MonopolyLite.Data;

namespace MonopolyLite.State
{
    public class GameState
    {
        public PlayerState Player { get; }
        public BoardState Board { get; }
        public BoardDef BoardDef { get; }

        public GameState(BoardDef boardDef, int startingDice, int diceCap)
        {
            BoardDef = boardDef;
            Player = new PlayerState(startingDice, diceCap);
            Board = new BoardState(boardDef.landmarks);
        }
    }
}
