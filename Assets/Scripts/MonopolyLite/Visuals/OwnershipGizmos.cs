// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEngine;
//
// namespace MonopolyLite.EditorTools
// {
//     [CustomEditor(typeof(GameDriver))]
//     public class OwnershipGizmos : Editor
//     {
//         private void OnSceneGUI()
//         {
//             GameDriver drv = (GameDriver)target;
//             if (drv == null || drv.Game == null) return;
//             MonopolyLiteGame g = drv.Game;
//             if (g.state.boardConfig == null) return;
//             BoardView view = FindObjectOfType<BoardView>();
//             if (view == null) return;
//             Vector3[] positions = BoardLayout.PerimeterExact(g.state.boardConfig.tiles.Length, view.sideLength);
//             for (int i = 0; i < g.state.tileOwner.Length; i++)
//             {
//                 int o = g.state.tileOwner[i];
//                 if (o < 0) continue;
//                 Vector3 pos = view.transform.TransformPoint(positions[i] + new Vector3(0, -0.6f, 0));
//                 Handles.color = Color.cyan;
//                 Handles.Label(pos, $"Owner:P{o}");
//             }
//         }
//     }
// }
// #endif

