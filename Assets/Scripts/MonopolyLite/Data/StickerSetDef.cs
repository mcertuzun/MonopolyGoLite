using System;
namespace MonopolyLite.Data
{
    [Serializable]
    public struct StickerSetDef
    {
        public string name;
        public int stickerCount;
        public int coinReward;
        public int diceReward;
    }
}
