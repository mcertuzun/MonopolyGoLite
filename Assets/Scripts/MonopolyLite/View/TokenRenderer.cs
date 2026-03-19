using MonopolyLite;
using UnityEngine;

namespace MonopolyLite.View
{
    public class TokenRenderer : MonoBehaviour
    {
        SpriteRenderer _sr;
        const int TokenPixelSize = 32;

        public void Initialize(Color color, float worldSize)
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = Sprites.Circle(TokenPixelSize, color);
            _sr.transform.localScale = Vector3.one * (worldSize * 0.4f / (TokenPixelSize / 100f));
            _sr.sortingOrder = 10;
        }

        public void MoveTo(Vector3 position)
        {
            transform.position = position + Vector3.back * 0.1f;
        }
    }
}
