using MonopolyLite.Data;

namespace MonopolyLite.Logic
{
    public class MockTargetProvider : ITargetProvider
    {
        static readonly string[] BotNames =
        {
            "Ali", "Ayse", "Mehmet", "Fatma", "Emre", "Zeynep",
            "Burak", "Elif", "Cem", "Deniz", "Gul", "Hakan"
        };

        static readonly ColorGroup[] LandmarkGroups =
        {
            ColorGroup.Brown, ColorGroup.LightBlue, ColorGroup.Pink, ColorGroup.Orange,
            ColorGroup.Red, ColorGroup.Yellow, ColorGroup.Green, ColorGroup.Blue
        };

        static readonly string[] LandmarkNames =
        {
            "Monument A", "Monument B", "Monument C", "Monument D",
            "Monument E", "Monument F", "Monument G", "Monument H"
        };

        RNG _rng;

        public MockTargetProvider(int seed)
        {
            _rng = new RNG((uint)seed);
        }

        public TargetProfile GetRandomTarget(int boardIndex)
        {
            string name = BotNames[_rng.Next(0, BotNames.Length)];
            int shields = _rng.Next(0, 4);
            int landmarkCount = _rng.Next(4, LandmarkGroups.Length + 1);

            var landmarks = new TargetLandmark[landmarkCount];
            int totalNW = 0;

            for (int i = 0; i < landmarkCount; i++)
            {
                int level = _rng.Next(1, 6);
                int nw = level * 100 * (boardIndex + 1);
                totalNW += nw;
                landmarks[i] = new TargetLandmark
                {
                    colorGroup = LandmarkGroups[i],
                    name = LandmarkNames[i],
                    level = level,
                };
            }

            return new TargetProfile
            {
                displayName = name,
                shields = shields,
                netWorth = totalNW,
                landmarks = landmarks,
            };
        }
    }
}
