using NUnit.Framework;
using MonopolyLite.Data;
using MonopolyLite.State;
using MonopolyLite.Logic;

namespace MonopolyLite.Tests
{
    public class StickerSystemTests
    {
        StickerSystem _system;
        StickerState _state;
        AlbumDef _album;

        [SetUp]
        public void SetUp()
        {
            _system = new StickerSystem(42);
            _state = new StickerState();
            _album = new AlbumDef
            {
                name = "Test Album",
                sets = new StickerSetDef[]
                {
                    new StickerSetDef { name = "Set A", stickerCount = 3, coinReward = 500, diceReward = 25 },
                    new StickerSetDef { name = "Set B", stickerCount = 3, coinReward = 1000, diceReward = 50 },
                },
                stickers = new StickerDef[]
                {
                    new StickerDef { id = 0, name = "S0", setIndex = 0, rarity = StickerRarity.Star1 },
                    new StickerDef { id = 1, name = "S1", setIndex = 0, rarity = StickerRarity.Star2 },
                    new StickerDef { id = 2, name = "S2", setIndex = 0, rarity = StickerRarity.Star3 },
                    new StickerDef { id = 3, name = "S3", setIndex = 1, rarity = StickerRarity.Star1 },
                    new StickerDef { id = 4, name = "S4", setIndex = 1, rarity = StickerRarity.Star2 },
                    new StickerDef { id = 5, name = "S5", setIndex = 1, rarity = StickerRarity.Star4 },
                },
            };
        }

        [Test] public void GrantRandom_ReturnsValidStickerId()
        { int id = _system.GrantRandom(_state, _album); Assert.GreaterOrEqual(id, 0); Assert.Less(id, _album.stickers.Length); }

        [Test] public void GrantRandom_AddsStickerToState()
        { int id = _system.GrantRandom(_state, _album); Assert.IsTrue(_state.HasSticker(id)); Assert.AreEqual(1, _state.GetStickerCount(id)); }

        [Test] public void GrantRandom_DuplicateAddsDuplicateStars()
        {
            int id = _system.GrantRandom(_state, _album);
            var def = _album.stickers[id];
            _state.AddSticker(id, def.rarity);
            Assert.AreEqual(2, _state.GetStickerCount(id));
            Assert.AreEqual((int)def.rarity, _state.DuplicateStars);
        }

        [Test] public void IsSetComplete_FalseWhenMissing()
        {
            _state.AddSticker(0, StickerRarity.Star1);
            _state.AddSticker(1, StickerRarity.Star2);
            Assert.IsFalse(_system.IsSetComplete(_state, _album, 0));
        }

        [Test] public void IsSetComplete_TrueWhenAllOwned()
        {
            _state.AddSticker(0, StickerRarity.Star1);
            _state.AddSticker(1, StickerRarity.Star2);
            _state.AddSticker(2, StickerRarity.Star3);
            Assert.IsTrue(_system.IsSetComplete(_state, _album, 0));
        }

        [Test] public void GetSetOwnedCount_ReturnsCorrect()
        {
            _state.AddSticker(0, StickerRarity.Star1);
            _state.AddSticker(2, StickerRarity.Star3);
            Assert.AreEqual(2, _system.GetSetOwnedCount(_state, _album, 0));
        }

        [Test] public void IsAlbumComplete_FalseWhenIncomplete()
        {
            _state.AddSticker(0, StickerRarity.Star1);
            _state.AddSticker(1, StickerRarity.Star2);
            _state.AddSticker(2, StickerRarity.Star3);
            Assert.IsFalse(_system.IsAlbumComplete(_state, _album));
        }

        [Test] public void IsAlbumComplete_TrueWhenAllStickersOwned()
        {
            for (int i = 0; i < 6; i++)
                _state.AddSticker(i, _album.stickers[i].rarity);
            Assert.IsTrue(_system.IsAlbumComplete(_state, _album));
        }

        [Test] public void GrantRandom_Deterministic()
        {
            var s1 = new StickerSystem(99); var s2 = new StickerSystem(99);
            var st1 = new StickerState(); var st2 = new StickerState();
            int id1 = s1.GrantRandom(st1, _album);
            int id2 = s2.GrantRandom(st2, _album);
            Assert.AreEqual(id1, id2);
        }
    }
}
