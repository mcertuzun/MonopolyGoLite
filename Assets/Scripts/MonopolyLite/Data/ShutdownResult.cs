using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct ShutdownResult
    {
        public bool Shielded;
        public int CoinsEarned;
        public ColorGroup TargetedLandmark;
        public string TargetName;
    }
}
