using MonopolyLite.Data;

namespace MonopolyLite.State
{
    public class GameState
    {
        public PlayerState Player { get; }
        public BoardState Board { get; private set; }
        public BoardDef BoardDef { get; private set; }
        public ProgressionState Progression { get; }

        public GameState(BoardDef boardDef, int startingDice, int diceCap,
                         ProgressionState progression = null)
        {
            BoardDef = boardDef;
            Player = new PlayerState(startingDice, diceCap);
            Board = new BoardState(boardDef.landmarks);
            Progression = progression;
        }

        public void TransitionToBoard(BoardDef newBoard)
        {
            BoardDef = newBoard;
            Board = new BoardState(newBoard.landmarks);
            Player.Position = 0;
        }
    }
}
