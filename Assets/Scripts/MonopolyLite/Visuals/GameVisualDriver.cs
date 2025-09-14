using UnityEngine;

namespace MonopolyLite
{
    public class GameVisualDriver : MonoBehaviour
    {
        public GameDriver driver;
        public BoardView boardView;
        public float tokenOffset = 0.3f;
        public float tweenSpeed = 14f;
        private float cachedSideLen = -1f;
        private int cachedTileCount = -1;
        private float cachedTileSize = -1f;
        private Vector3[] localTilePositions;

        private TokenView[] tokens;
        private Vector3[] worldTilePositions;

        private void Update()
        {
            if (driver == null) driver = FindObjectOfType<GameDriver>();
            if (driver == null || driver.Game == null) return;
            if (boardView == null) boardView = FindObjectOfType<BoardView>();
            if (boardView == null || driver.Game.state.pos == null) return;

            MonopolyLiteGame game = driver.Game;

            if (tokens == null)
            {
                int count = game.state.playerCount;
                tokens = new TokenView[count];
                Color[] palette =
                { new(0.2f, 0.6f, 1f), new(1f, 0.5f, 0.2f), new(0.3f, 0.9f, 0.4f), new(0.9f, 0.3f, 0.7f) };
                for (int i = 0; i < count; i++)
                {
                    GameObject go = new($"Token_{i}");
                    go.transform.SetParent(transform, false);
                    TokenView tv = go.AddComponent<TokenView>();
                    tv.Init(i, palette[i % palette.Length], "Default", 50 + i * 2);
                    tokens[i] = tv;
                }
            }

            if (NeedsRebuild(game))
                RebuildPositions(game);

            int[] counts = new int[worldTilePositions.Length];
            for (int p = 0; p < game.state.playerCount; p++)
                counts[game.state.pos[p]]++;

            int[] seen = new int[worldTilePositions.Length];

            for (int p = 0; p < game.state.playerCount && p < tokens.Length; p++)
            {
                int tile = game.state.pos[p];
                int rank = seen[tile]++;
                Vector3 baseWorld = worldTilePositions[tile];

                Vector3 offset = Vector3.zero;
                if (counts[tile] > 1)
                {
                    float angle = rank / Mathf.Max(1f, counts[tile]) * Mathf.PI * 2f;
                    offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * (tokenOffset * boardView.tileSize);
                }

                Vector3 target = baseWorld + offset;

                if (tokens[p].transform.position == Vector3.zero && Time.frameCount < 5)
                    tokens[p].transform.position = target;
                else
                    tokens[p].transform.position = Vector3.Lerp(tokens[p].transform.position, target, Time.deltaTime * tweenSpeed);
            }
        }

        private bool NeedsRebuild(MonopolyLiteGame game)
        {
            return localTilePositions == null
                   || game.state.boardConfig.tiles.Length != cachedTileCount
                   || !Mathf.Approximately(boardView.sideLength, cachedSideLen)
                   || !Mathf.Approximately(boardView.tileSize, cachedTileSize);
        }

        private void RebuildPositions(MonopolyLiteGame game)
        {
            cachedTileCount = game.state.boardConfig.tiles.Length;
            cachedSideLen = boardView.sideLength;
            cachedTileSize = boardView.tileSize;

            localTilePositions = BoardLayout.Perimeter(cachedTileCount, cachedSideLen, cachedTileSize, 0.1f);
            worldTilePositions = new Vector3[cachedTileCount];
            for (int i = 0; i < cachedTileCount; i++)
                worldTilePositions[i] = boardView.transform.TransformPoint(localTilePositions[i]);
        }
    }
}