using System;

namespace MonopolyLite
{
    public static class DataCache
    {
        private static LiveOpsConfig cached;
        private static DateTime expiry;

        public static LiveOpsConfig GetLiveOpsOr(Func<LiveOpsConfig> loadDefault)
        {
            if (cached != null && DateTime.UtcNow < expiry) return cached;
            LiveOpsConfig def = loadDefault();
            cached = def;
            expiry = DateTime.UtcNow.AddSeconds(def.ttlSeconds);
            return cached;
        }
    }
}