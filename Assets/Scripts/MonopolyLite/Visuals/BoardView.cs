using TMPro;
using UnityEngine;

namespace MonopolyLite
{
    public class BoardView : MonoBehaviour
    {
        public BoardConfig boardConfig;
        public float sideLength = 12f;
        public float tileSize = 1.8f;
        public float nameOffset = 1.1f;
        public string sortingLayer = "Default";
        public int tileOrder = 0;
        public int textOrder = 2;
        public Color propertyColor = new(0.20f, 0.62f, 0.86f);
        public Color taxColor = new(0.86f, 0.31f, 0.31f);
        public Color chestColor = new(0.47f, 0.86f, 0.47f);
        public Color goColor = new(0.95f, 0.84f, 0.34f);
        public Color jailColor = new(0.75f, 0.55f, 0.35f);
        public Color gotoJailColor = new(0.60f, 0.35f, 0.75f);
        private Transform root;

        private void Start()
        {
            Build();
        }

        public void Init(BoardConfig b)
        {
            boardConfig = b;
        }

        public void Build()
        {
            if (root != null) DestroyImmediate(root.gameObject);
            root = new GameObject("Tiles").transform;
            root.SetParent(transform, false);
            Vector3[] positions = BoardLayout.Perimeter(boardConfig.tiles.Length, sideLength, tileSize, 0.1f);
            for (int i = 0; i < boardConfig.tiles.Length; i++)
            {
                Tile t = boardConfig.tiles[i];
                GameObject go = new($"Tile_{i}_{t.name}");
                go.transform.SetParent(root, false);
                go.transform.localPosition = positions[i];
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = RuntimeSpriteFactory.MakeSquareSprite(64, ColorForTile(t.type));
                sr.sortingLayerName = sortingLayer;
                sr.sortingOrder = tileOrder;
                go.transform.localScale = new Vector3(tileSize, tileSize, 1f);
                GameObject labelGO = new("Label");
                labelGO.transform.SetParent(go.transform, false);
                labelGO.transform.localPosition = new Vector3(0f, nameOffset, 0f);
                TextMeshPro tmp = labelGO.AddComponent<TextMeshPro>();
                tmp.text = t.name;
                tmp.fontSize = 2.5f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.sortingLayerID = sr.sortingLayerID;
                tmp.sortingOrder = textOrder;
                tmp.enableWordWrapping = false;
                tmp.rectTransform.sizeDelta = new Vector2(6f, 1.5f);
            }
        }

        private Color ColorForTile(TileType tt)
        {
            switch (tt)
            {
                case TileType.Property: return propertyColor;
                case TileType.Tax: return taxColor;
                case TileType.Chest: return chestColor;
                case TileType.Go: return goColor;
                case TileType.Jail: return jailColor;
                case TileType.GoToJail: return gotoJailColor;
                default: return Color.white;
            }
        }
    }
}