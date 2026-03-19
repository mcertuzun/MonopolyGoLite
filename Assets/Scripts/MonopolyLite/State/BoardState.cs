using System.Collections.Generic;
using MonopolyLite.Data;

namespace MonopolyLite.State
{
    public class BoardState
    {
        readonly Dictionary<ColorGroup, int> _landmarkLevels = new();
        readonly LandmarkDef[] _landmarks;

        public int ChanceDrawIndex { get; set; }
        public int CommunityChestDrawIndex { get; set; }

        public BoardState(LandmarkDef[] landmarks)
        {
            _landmarks = landmarks;
            foreach (var lm in landmarks)
                _landmarkLevels[lm.colorGroup] = 0;
        }

        public int GetLandmarkLevel(ColorGroup group)
        {
            return _landmarkLevels.TryGetValue(group, out int level) ? level : 0;
        }

        public void SetLandmarkLevel(ColorGroup group, int level)
        {
            _landmarkLevels[group] = System.Math.Clamp(level, 0, 5);
        }

        public bool IsComplete()
        {
            foreach (var lm in _landmarks)
            {
                if (GetLandmarkLevel(lm.colorGroup) < 5) return false;
            }
            return true;
        }
    }
}
