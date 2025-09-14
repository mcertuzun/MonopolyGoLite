using TMPro;
using UnityEngine;

namespace MonopolyLite
{
    public class StatsView : MonoBehaviour
    {
        public GameDriver driver;
        public Vector3 worldAnchor = new(0f, 7f, 0f);
        public float lineSpacing = 0.8f;
        public float fontSize = 3f;
        private TextMeshPro[] lines;

        private void LateUpdate()
        {
            if (driver == null) driver = FindObjectOfType<GameDriver>();
            if (driver == null || driver.Game == null) return;
            MonopolyLiteGame g = driver.Game;
            int n = g.state.playerCount + 3;
            Ensure(n);
            Camera cam = Camera.main;
            Vector3 head = (cam != null ? cam.transform.position : Vector3.zero) + worldAnchor;
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].transform.position = head + new Vector3(-6f, -i * lineSpacing, 0f);
                lines[i].sortingOrder = 100 + i;
            }

            lines[0].text = $"Turn P{g.state.currentPlayer} Frame:{g.Frame} Dice:{g.state.lastD1}+{g.state.lastD2}";
            lines[1].text = $"Charges:{g.state.diceCharges}/{g.state.diceChargeCap} Mult:x{g.state.gainMultiplier}";
            for (int p = 0; p < g.state.playerCount; p++)
            {
                int tile = g.state.pos[p];
                Tile t = g.state.boardConfig.tiles[tile];
                lines[2 + p].text = $"P{p} Cash:{g.state.cash[p]} Tile:{tile} {t.name}";
            }

            lines[n - 1].text = $"";
        }

        private void Ensure(int n)
        {
            if (lines != null && lines.Length == n) return;
            if (lines != null)
                for (int i = 0; i < lines.Length; i++)
                    if (lines[i] != null)
                        Destroy(lines[i].gameObject);
            lines = new TextMeshPro[n];
            for (int i = 0; i < n; i++)
            {
                GameObject go = new("StatLine_" + i);
                go.transform.SetParent(transform, false);
                TextMeshPro t = go.AddComponent<TextMeshPro>();
                t.fontSize = fontSize;
                t.alignment = TextAlignmentOptions.Left;
                lines[i] = t;
            }
        }
    }
}