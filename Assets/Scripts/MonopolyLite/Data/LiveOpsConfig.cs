using System;

namespace MonopolyLite
{
    [Serializable]
    public class LiveOpsConfig
    {
        public int playerCount = 2;
        public int goPayoutMultiplier = 1;
        public float rentMultiplier = 1f;
        public string version = "1.0.0";
        public int ttlSeconds = 3600;
    }
}