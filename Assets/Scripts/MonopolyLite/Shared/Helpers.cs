using UnityEngine;

namespace MonopolyLite
{
    public static class Sprites
    {
        public static Sprite Square(int px, Color c)
        {
            Texture2D t = new(px, px, TextureFormat.RGBA32, false);
            Color[] a = new Color[px * px];
            for (int i = 0; i < a.Length; i++) a[i] = c;
            t.SetPixels(a);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, px, px), new Vector2(0.5f, 0.5f), px);
        }

        public static Sprite Circle(int px, Color c)
        {
            Texture2D t = new(px, px, TextureFormat.RGBA32, false);
            float r = (px - 1) * 0.5f, r2 = r * r;
            for (int y = 0; y < px; y++)
            for (int x = 0; x < px; x++)
            {
                float dx = x - r, dy = y - r;
                t.SetPixel(x, y, dx * dx + dy * dy <= r2 ? c : new Color(0, 0, 0, 0));
            }

            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, px, px), new Vector2(0.5f, 0.5f), px);
        }
    }

    public static class Layout
    {
        public static Vector3[] Perimeter(int n, float side, float tile, float pad)
        {
            if (n < 4) n = 4;
            int bp = n / 4, r = n % 4;
            int cb = bp + (r > 0 ? 1 : 0), cl = bp + (r > 1 ? 1 : 0), ct = bp + (r > 2 ? 1 : 0), cr = bp;
            Vector3[] p = new Vector3[n];
            float half = side * 0.5f;
            float inner = half - tile * 0.5f - pad;
            int i = 0;
            p[i++] = new Vector3(+inner, -inner, 0);
            for (int k = 1; k < cb; k++)
            {
                float t = k / (float)cb;
                p[i++] = new Vector3(Mathf.Lerp(+inner, -inner, t), -inner, 0);
            }

            p[i++] = new Vector3(-inner, -inner, 0);
            for (int k = 1; k < cl; k++)
            {
                float t = k / (float)cl;
                p[i++] = new Vector3(-inner, Mathf.Lerp(-inner, +inner, t), 0);
            }

            p[i++] = new Vector3(-inner, +inner, 0);
            for (int k = 1; k < ct; k++)
            {
                float t = k / (float)ct;
                p[i++] = new Vector3(Mathf.Lerp(-inner, +inner, t), +inner, 0);
            }

            p[i++] = new Vector3(+inner, +inner, 0);
            for (int k = 1; k < cr; k++)
            {
                float t = k / (float)cr;
                p[i++] = new Vector3(+inner, Mathf.Lerp(+inner, -inner, t), 0);
            }

            while (i < n) p[i++] = new Vector3(+inner, -inner, 0);
            return p;
        }
    }

    public struct RNG
    {
        private ulong s, inc;

        public RNG(uint seed, uint seq = 54u)
        {
            s = 0;
            inc = ((ulong)seq << 1) | 1;
            NextUInt();
            s += seed;
            NextUInt();
        }

        public uint NextUInt()
        {
            ulong o = s;
            s = o * 6364136223846793005UL + inc;
            uint x = (uint)(((o >> 18) ^ o) >> 27);
            uint r = (uint)(o >> 59);
            return (x >> (int)r) | (x << (int)(-r & 31));
        }

        public int Next(int a, int b)
        {
            uint range = (uint)(b - a);
            return a + (int)(NextUInt() % range);
        }
    }
}