using MonopolyLite;
using MonopolyLite.Data;
using UnityEngine;

namespace MonopolyLite.View
{
    public class BoardRenderer : MonoBehaviour
    {
        GameObject[] _tileObjects;
        const int TilePixelSize = 64;
        const float TilePad = 0.1f;

        public void Render(BoardDef board)
        {
            _tileObjects = new GameObject[board.tiles.Length];
            var positions = Layout.Perimeter(board.tiles.Length, board.sideLength, board.tileSize, TilePad);

            for (int i = 0; i < board.tiles.Length; i++)
            {
                var tile = board.tiles[i];
                var go = new GameObject($"Tile_{i}_{tile.name}");
                go.transform.SetParent(transform);
                go.transform.position = new Vector3(positions[i].x, positions[i].y, 0);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Sprites.Square(TilePixelSize, GetTileColor(tile));
                sr.transform.localScale = Vector3.one * (board.tileSize / (TilePixelSize / 100f));
                sr.sortingOrder = 0;

                _tileObjects[i] = go;
            }
        }

        public Vector3 GetTilePosition(int index)
        {
            if (_tileObjects == null || index < 0 || index >= _tileObjects.Length)
                return Vector3.zero;
            return _tileObjects[index].transform.position;
        }

        static Color GetTileColor(TileDef tile)
        {
            return tile.colorGroup switch
            {
                ColorGroup.Brown     => new Color(0.55f, 0.27f, 0.07f),
                ColorGroup.LightBlue => new Color(0.68f, 0.85f, 0.90f),
                ColorGroup.Pink      => new Color(0.85f, 0.44f, 0.84f),
                ColorGroup.Orange    => new Color(1.0f, 0.65f, 0.0f),
                ColorGroup.Red       => new Color(0.9f, 0.1f, 0.1f),
                ColorGroup.Yellow    => new Color(1.0f, 0.95f, 0.0f),
                ColorGroup.Green     => new Color(0.0f, 0.7f, 0.0f),
                ColorGroup.Blue      => new Color(0.0f, 0.0f, 0.8f),
                _ => tile.type switch
                {
                    TileType.Railroad       => new Color(0.3f, 0.3f, 0.3f),
                    TileType.Chance         => new Color(1.0f, 0.8f, 0.2f),
                    TileType.CommunityChest => new Color(0.2f, 0.6f, 1.0f),
                    TileType.Tax            => new Color(0.6f, 0.0f, 0.0f),
                    _                       => new Color(0.9f, 0.9f, 0.85f),
                }
            };
        }
    }
}
