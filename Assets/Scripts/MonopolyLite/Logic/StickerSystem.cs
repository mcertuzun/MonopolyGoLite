using MonopolyLite;
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public class StickerSystem
    {
        RNG _rng;

        public StickerSystem(int seed)
        {
            _rng = new RNG((uint)seed);
        }

        public int GrantRandom(StickerState state, AlbumDef album)
        {
            int idx = _rng.Next(0, album.stickers.Length);
            var sticker = album.stickers[idx];
            state.AddSticker(sticker.id, sticker.rarity);
            return sticker.id;
        }

        public bool IsSetComplete(StickerState state, AlbumDef album, int setIndex)
        {
            foreach (var s in album.stickers)
            {
                if (s.setIndex == setIndex && !state.HasSticker(s.id))
                    return false;
            }
            return true;
        }

        public int GetSetOwnedCount(StickerState state, AlbumDef album, int setIndex)
        {
            int count = 0;
            foreach (var s in album.stickers)
            {
                if (s.setIndex == setIndex && state.HasSticker(s.id))
                    count++;
            }
            return count;
        }

        public bool IsAlbumComplete(StickerState state, AlbumDef album)
        {
            foreach (var s in album.stickers)
            {
                if (!state.HasSticker(s.id))
                    return false;
            }
            return true;
        }
    }
}
