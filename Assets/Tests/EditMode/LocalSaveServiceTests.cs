using NUnit.Framework;
using MonopolyLite.Config;
using MonopolyLite.Data;

namespace MonopolyLite.Tests
{
    public class LocalSaveServiceTests
    {
        LocalSaveService _service;
        string _testPath;

        [SetUp]
        public void SetUp()
        {
            _testPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"monopoly_test_{System.Guid.NewGuid()}.json");
            _service = new LocalSaveService(_testPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (System.IO.File.Exists(_testPath))
                System.IO.File.Delete(_testPath);
        }

        [Test]
        public void HasSave_FalseWhenNoFile()
        {
            Assert.IsFalse(_service.HasSave());
        }

        [Test]
        public void Save_CreateFile()
        {
            var data = new SaveData { coins = 100 };
            _service.Save(data);
            Assert.IsTrue(_service.HasSave());
            Assert.IsTrue(System.IO.File.Exists(_testPath));
        }

        [Test]
        public void SaveAndLoad_RoundTrip()
        {
            var data = new SaveData
            {
                coins = 5000,
                dice = 80,
                netWorth = 1200,
                currentBoardIndex = 1,
                loginStreak = 3,
                claimedMilestones = new int[] { 0, 1 },
                unlockedMultipliers = new int[] { 1, 2 },
                landmarkLevels = new LandmarkSaveEntry[]
                {
                    new LandmarkSaveEntry { colorGroup = 1, level = 3 },
                },
                totalRolls = 42,
            };

            _service.Save(data);
            var loaded = _service.Load();

            Assert.AreEqual(5000, loaded.coins);
            Assert.AreEqual(80, loaded.dice);
            Assert.AreEqual(1200, loaded.netWorth);
            Assert.AreEqual(1, loaded.currentBoardIndex);
            Assert.AreEqual(3, loaded.loginStreak);
            Assert.AreEqual(42, loaded.totalRolls);
            Assert.AreEqual(2, loaded.claimedMilestones.Length);
            Assert.AreEqual(1, loaded.landmarkLevels.Length);
            Assert.AreEqual(3, loaded.landmarkLevels[0].level);
        }

        [Test]
        public void Delete_RemovesFile()
        {
            _service.Save(new SaveData { coins = 1 });
            Assert.IsTrue(_service.HasSave());
            _service.Delete();
            Assert.IsFalse(_service.HasSave());
        }

        [Test]
        public void Save_SetsLastSavedAt()
        {
            _service.Save(new SaveData());
            var loaded = _service.Load();
            Assert.IsNotNull(loaded.lastSavedAt);
            Assert.IsNotEmpty(loaded.lastSavedAt);
        }
    }
}
