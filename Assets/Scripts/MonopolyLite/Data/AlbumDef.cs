using System;
namespace MonopolyLite.Data
{
    [Serializable]
    public class AlbumDef
    {
        public string name;
        public StickerSetDef[] sets;
        public StickerDef[] stickers;
    }
}
