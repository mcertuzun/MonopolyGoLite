using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class BoardProgressionSystem
    {
        readonly string[] _boardOrder;

        public BoardProgressionSystem(string[] boardOrder)
        {
            _boardOrder = boardOrder;
        }

        public bool HasNextBoard(int currentBoardIndex)
        {
            return currentBoardIndex + 1 < _boardOrder.Length;
        }

        public string GetCurrentBoardId(int currentBoardIndex)
        {
            return _boardOrder[currentBoardIndex];
        }

        public string GetNextBoardId(int currentBoardIndex)
        {
            return _boardOrder[currentBoardIndex + 1];
        }

        public bool AdvanceBoard(ProgressionState progression)
        {
            if (!HasNextBoard(progression.CurrentBoardIndex))
                return false;

            progression.CurrentBoardIndex++;
            return true;
        }
    }
}
