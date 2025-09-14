using UnityEngine;

namespace MonopolyLite
{
    public static class BoardLayout
    {
        private static void Distribute(int total, out int bottom, out int left, out int top, out int right)
        {
            if (total < 4) total = 4;
            int basePer = total / 4;
            int rem = total % 4;
            bottom = basePer + (rem > 0 ? 1 : 0);
            left = basePer + (rem > 1 ? 1 : 0);
            top = basePer + (rem > 2 ? 1 : 0);
            right = basePer;
        }

        public static Vector3[] Perimeter(int tileCount, float sideLength, float tileSize = 1.8f, float pad = 0.1f)
        {
            Distribute(tileCount, out int cb, out int cl, out int ct, out int cr);
            Vector3[] p = new Vector3[tileCount];
            float half = sideLength * 0.5f;
            float inner = half - tileSize * 0.5f - pad;
            int idx = 0;

            p[idx++] = new Vector3(+inner, -inner, 0f);
            for (int i = 1; i < cb; i++)
            {
                float t = i / (float)cb;
                float x = Mathf.Lerp(+inner, -inner, t);
                p[idx++] = new Vector3(x, -inner, 0f);
            }

            p[idx++] = new Vector3(-inner, -inner, 0f);
            for (int i = 1; i < cl; i++)
            {
                float t = i / (float)cl;
                float y = Mathf.Lerp(-inner, +inner, t);
                p[idx++] = new Vector3(-inner, y, 0f);
            }

            p[idx++] = new Vector3(-inner, +inner, 0f);
            for (int i = 1; i < ct; i++)
            {
                float t = i / (float)ct;
                float x = Mathf.Lerp(-inner, +inner, t);
                p[idx++] = new Vector3(x, +inner, 0f);
            }

            p[idx++] = new Vector3(+inner, +inner, 0f);
            for (int i = 1; i < cr; i++)
            {
                float t = i / (float)cr;
                float y = Mathf.Lerp(+inner, -inner, t);
                p[idx++] = new Vector3(+inner, y, 0f);
            }

            while (idx < tileCount) p[idx++] = new Vector3(+inner, -inner, 0f);
            return p;
        }
    }
}