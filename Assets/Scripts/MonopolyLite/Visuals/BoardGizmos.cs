#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MonopolyLite.EditorTools
{
    [CustomEditor(typeof(BoardView))]
    public class BoardGizmos : Editor
    {
        private void OnSceneGUI()
        {
            BoardView view = (BoardView)target;
            if (view == null || view.boardConfig == null) return;
            Vector3[] positions = BoardLayout.Perimeter(view.boardConfig.tiles.Length, view.sideLength);
            Handles.color = Color.yellow;
            for (int i = 0; i < view.boardConfig.tiles.Length; i++)
            {
                Tile t = view.boardConfig.tiles[i];
                Vector3 pos = view.transform.TransformPoint(positions[i] + Vector3.up * 0.3f);
                string price = t.type == TileType.Property ? $"${t.price}" : "";
                Handles.Label(pos, $"[{i}] {t.name} {price}");
            }
        }
    }
}
#endif