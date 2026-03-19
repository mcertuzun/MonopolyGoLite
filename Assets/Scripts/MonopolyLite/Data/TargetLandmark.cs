using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct TargetLandmark
    {
        public ColorGroup colorGroup;
        public string name;
        public int level;
    }
}
