using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class BoardProgressionSystemTests
    {
        BoardProgressionSystem _system;
        ProgressionState _progression;

        [SetUp]
        public void SetUp()
        {
            _system = new BoardProgressionSystem(
                new string[] { "board_01_istanbul", "board_02_paris", "board_03_tokyo" }
            );
            _progression = new ProgressionState();
        }

        [Test]
        public void HasNextBoard_True_WhenNotLast()
        {
            Assert.IsTrue(_system.HasNextBoard(_progression.CurrentBoardIndex));
        }

        [Test]
        public void HasNextBoard_False_WhenLast()
        {
            _progression.CurrentBoardIndex = 2;

            Assert.IsFalse(_system.HasNextBoard(_progression.CurrentBoardIndex));
        }

        [Test]
        public void GetNextBoardId_ReturnsNext()
        {
            Assert.AreEqual("board_02_paris", _system.GetNextBoardId(_progression.CurrentBoardIndex));
        }

        [Test]
        public void GetCurrentBoardId_ReturnsCurrent()
        {
            Assert.AreEqual("board_01_istanbul", _system.GetCurrentBoardId(_progression.CurrentBoardIndex));
        }

        [Test]
        public void AdvanceBoard_IncrementsIndex()
        {
            _system.AdvanceBoard(_progression);

            Assert.AreEqual(1, _progression.CurrentBoardIndex);
        }

        [Test]
        public void AdvanceBoard_StopsAtLast()
        {
            _progression.CurrentBoardIndex = 2;

            bool advanced = _system.AdvanceBoard(_progression);

            Assert.IsFalse(advanced);
            Assert.AreEqual(2, _progression.CurrentBoardIndex);
        }

        [Test]
        public void AdvanceBoard_SequentialProgression()
        {
            Assert.IsTrue(_system.AdvanceBoard(_progression));
            Assert.AreEqual("board_02_paris", _system.GetCurrentBoardId(_progression.CurrentBoardIndex));

            Assert.IsTrue(_system.AdvanceBoard(_progression));
            Assert.AreEqual("board_03_tokyo", _system.GetCurrentBoardId(_progression.CurrentBoardIndex));

            Assert.IsFalse(_system.AdvanceBoard(_progression));
        }

        [Test]
        public void SingleBoard_NoNext()
        {
            var system = new BoardProgressionSystem(new string[] { "board_01_istanbul" });
            var progression = new ProgressionState();

            Assert.IsFalse(system.HasNextBoard(progression.CurrentBoardIndex));
        }
    }
}
