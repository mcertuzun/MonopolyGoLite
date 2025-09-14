#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

namespace MonopolyLite.EditorTools
{
    public static class CreatePrefabs
    {
        [MenuItem("MonopolyLite/Create Tile & Token Prefabs")]
        public static void Create()
        {
            GameObject tile = new("TileVisual");
            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.MakeSquareSprite(64, new Color(0.2f, 0.62f, 0.86f));
            GameObject label = new("Label");
            label.transform.SetParent(tile.transform, false);
            label.transform.localPosition = new Vector3(0, 1.1f, 0);
            TextMeshPro tmp = label.AddComponent<TextMeshPro>();
            tmp.text = "Sample";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 2.5f;
            PrefabUtility.SaveAsPrefabAsset(tile, "Assets/TileVisual.prefab");
            GameObject.DestroyImmediate(tile);

            GameObject token = new("TokenVisual");
            SpriteRenderer tsr = token.AddComponent<SpriteRenderer>();
            tsr.sprite = RuntimeSpriteFactory.MakeCircleSprite(64, Color.cyan);
            GameObject tlabel = new("Label");
            tlabel.transform.SetParent(token.transform, false);
            tlabel.transform.localPosition = new Vector3(0, 0.9f, 0);
            TextMeshPro ttmp = tlabel.AddComponent<TextMeshPro>();
            ttmp.text = "P0";
            ttmp.alignment = TextAlignmentOptions.Center;
            ttmp.fontSize = 2.5f;
            PrefabUtility.SaveAsPrefabAsset(token, "Assets/TokenVisual.prefab");
            GameObject.DestroyImmediate(token);

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("MonopolyLite", "Prefabs created.", "OK");
        }
    }
}
#endif