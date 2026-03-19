using MonopolyLite.Data;
using UnityEngine;

namespace MonopolyLite.Config
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "MonopolyLite/BoardConfig")]
    public class BoardConfig : ScriptableObject
    {
        public BoardDef board;
    }
}
