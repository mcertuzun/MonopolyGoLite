using UnityEngine;

namespace MonopolyLite
{
    public static class RuntimeSpriteFactory
    {
        public static Sprite MakeSquareSprite(int size, Color color)
        {
            Texture2D tex = new(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite MakeCircleSprite(int size, Color color)
        {
            Texture2D tex = new(size, size, TextureFormat.RGBA32, false);
            float c = (size - 1) * 0.5f;
            float r2 = c * c;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - c;
                float dy = y - c;
                bool inside = dx * dx + dy * dy <= r2;
                tex.SetPixel(x, y, inside ? color : new Color(0, 0, 0, 0));
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}