using UnityEngine;

namespace MonopolyLite
{
    public static class JsonLoader
    {
        public static BoardConfig LoadBoardFromJson(string path)
        {
            return JsonUtility.FromJson<BoardConfig>(Resources.Load<TextAsset>(path).text);
        }

        public static LiveOpsConfig LoadLiveOpsFromJson(string path)
        {
            return JsonUtility.FromJson<LiveOpsConfig>(Resources.Load<TextAsset>(path).text);
        }
    }
}