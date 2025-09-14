using TMPro;
using UnityEngine;

namespace MonopolyLite
{
    public class TokenView : MonoBehaviour
    {
        public int playerIndex;
        public Color color = Color.cyan;
        public string sortingLayer = "Default";
        public int spriteOrder = 3;
        public int textOrder = 4;
        public float radius = 0.7f;
        private TextMeshPro label;
        private SpriteRenderer sr;

        public void Init(int index, Color c, string layer, int orderBase)
        {
            playerIndex = index;
            color = c;
            sortingLayer = layer;
            spriteOrder = orderBase;
            textOrder = orderBase + 1;
            Build();
        }

        private void Build()
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.MakeCircleSprite(64, color);
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = spriteOrder;
            transform.localScale = Vector3.one * radius;
            GameObject go = new("Label");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            label = go.AddComponent<TextMeshPro>();
            label.text = $"P{playerIndex}";
            label.fontSize = 2.5f;
            label.alignment = TextAlignmentOptions.Center;
            label.sortingLayerID = sr.sortingLayerID;
            label.sortingOrder = textOrder;
            label.enableWordWrapping = false;
            label.rectTransform.sizeDelta = new Vector2(3f, 1f);
        }
    }
}