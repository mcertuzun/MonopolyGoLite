using System;
namespace MonopolyLite.Data
{
    [Serializable]
    public struct StickerDef
    {
        public int id;
        public string name;
        public int setIndex;
        public StickerRarity rarity;
    }
}
