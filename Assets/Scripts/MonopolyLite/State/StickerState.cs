using System.Collections.Generic;
using MonopolyLite.Data;

namespace MonopolyLite.State
{
    public class StickerState
    {
        public Dictionary<int, int> OwnedStickers { get; private set; }
        public int DuplicateStars { get; set; }

        public StickerState()
        {
            OwnedStickers = new Dictionary<int, int>();
            DuplicateStars = 0;
        }

        public void AddSticker(int stickerId, StickerRarity rarity)
        {
            if (OwnedStickers.ContainsKey(stickerId))
            {
                OwnedStickers[stickerId]++;
                DuplicateStars += (int)rarity;
            }
            else
            {
                OwnedStickers[stickerId] = 1;
            }
        }

        public int GetStickerCount(int stickerId)
        {
            return OwnedStickers.TryGetValue(stickerId, out int count) ? count : 0;
        }

        public bool HasSticker(int stickerId)
        {
            return OwnedStickers.ContainsKey(stickerId);
        }

        public void LoadFromEntries(StickerSaveEntry[] entries)
        {
            OwnedStickers = new Dictionary<int, int>();
            if (entries == null) return;
            foreach (var e in entries)
                OwnedStickers[e.stickerId] = e.count;
        }
    }
}
