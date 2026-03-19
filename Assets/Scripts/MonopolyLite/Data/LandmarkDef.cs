using System;

namespace MonopolyLite.Data
{
    [Serializable]
    public struct LandmarkDef
    {
        public ColorGroup colorGroup;
        public string name;
        public int[] costs;      // cost per level [0..4] for levels 1-5
        public int[] nwPoints;   // net worth granted per level [0..4] for levels 1-5
    }
}
