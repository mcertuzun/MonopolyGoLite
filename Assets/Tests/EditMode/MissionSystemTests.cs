using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class MissionSystemTests
    {
        MissionDef[] _pool;
        MissionSystem _system;

        [SetUp]
        public void SetUp()
        {
            _pool = new MissionDef[]
            {
                new MissionDef { type = MissionType.RollDice, description = "Roll {0} times", target = 5, coinReward = 200, diceReward = 10 },
                new MissionDef { type = MissionType.BuildLandmark, description = "Build {0} landmark", target = 1, coinReward = 300, diceReward = 15 },
                new MissionDef { type = MissionType.CompleteHeist, description = "Complete {0} heist", target = 2, coinReward = 400, diceReward = 20 },
                new MissionDef { type = MissionType.EarnCoins, description = "Earn {0} coins", target = 1000, coinReward = 500, diceReward = 25 },
            };
            _system = new MissionSystem(_pool, 42);
        }

        [Test] public void GenerateDaily_ReturnsRequestedCount()
        { var missions = _system.GenerateDaily(3); Assert.AreEqual(3, missions.Length); }

        [Test] public void GenerateDaily_MissionsStartAtZeroProgress()
        { var missions = _system.GenerateDaily(3); foreach (var m in missions) Assert.AreEqual(0, m.Progress); }

        [Test] public void GenerateDaily_MissionsHaveTargets()
        { var missions = _system.GenerateDaily(3); foreach (var m in missions) Assert.Greater(m.Target, 0); }

        [Test] public void TrackProgress_IncrementsMatchingMissions()
        {
            var missions = new MissionProgress[]
            { new MissionProgress { Type = MissionType.RollDice, Target = 5, Progress = 0 } };
            _system.TrackProgress(missions, MissionType.RollDice, 3);
            Assert.AreEqual(3, missions[0].Progress);
        }

        [Test] public void TrackProgress_DoesNotIncrementOtherTypes()
        {
            var missions = new MissionProgress[]
            {
                new MissionProgress { Type = MissionType.RollDice, Target = 5, Progress = 0 },
                new MissionProgress { Type = MissionType.BuildLandmark, Target = 1, Progress = 0 },
            };
            _system.TrackProgress(missions, MissionType.RollDice, 2);
            Assert.AreEqual(2, missions[0].Progress);
            Assert.AreEqual(0, missions[1].Progress);
        }

        [Test] public void Completed_TrueWhenProgressReachesTarget()
        { var m = new MissionProgress { Type = MissionType.RollDice, Target = 5, Progress = 5 }; Assert.IsTrue(m.Completed); }

        [Test] public void AllCompleted_TrueWhenAllDone()
        {
            var missions = new MissionProgress[]
            {
                new MissionProgress { Type = MissionType.RollDice, Target = 5, Progress = 5 },
                new MissionProgress { Type = MissionType.BuildLandmark, Target = 1, Progress = 1 },
            };
            Assert.IsTrue(_system.AllCompleted(missions));
        }

        [Test] public void AllCompleted_FalseWhenAnyIncomplete()
        {
            var missions = new MissionProgress[]
            {
                new MissionProgress { Type = MissionType.RollDice, Target = 5, Progress = 5 },
                new MissionProgress { Type = MissionType.BuildLandmark, Target = 1, Progress = 0 },
            };
            Assert.IsFalse(_system.AllCompleted(missions));
        }

        [Test] public void GenerateDaily_DeterministicWithSameSeed()
        {
            var s1 = new MissionSystem(_pool, 99);
            var s2 = new MissionSystem(_pool, 99);
            var m1 = s1.GenerateDaily(3);
            var m2 = s2.GenerateDaily(3);
            for (int i = 0; i < 3; i++)
            { Assert.AreEqual(m1[i].Type, m2[i].Type); Assert.AreEqual(m1[i].Target, m2[i].Target); }
        }
    }
}
