using TMPro;
using UnityEngine;

namespace MonopolyLite
{
    public class DiceView : MonoBehaviour
    {
        public GameDriver driver;
        public float edgePadding = 0.6f;
        public float horizontalGap = 1.0f;
        public Color diceColor = Color.white;
        public Color textColor = Color.black;
        public int pixelSize = 64;
        public float scale = 1.2f;
        public int sortingOrder = 100;

        private SpriteRenderer aSR, bSR;
        private TextMeshPro aTMP, bTMP;
        private int shownA, shownB;

        private void Start()
        {
            GameObject goA = new("DieA");
            goA.transform.SetParent(transform, false);
            aSR = goA.AddComponent<SpriteRenderer>();
            aSR.sprite = RuntimeSpriteFactory.MakeSquareSprite(pixelSize, diceColor);
            aSR.sortingOrder = sortingOrder;
            goA.transform.localScale = Vector3.one * scale;
            GameObject la = new("LabelA");
            la.transform.SetParent(goA.transform, false);
            aTMP = la.AddComponent<TextMeshPro>();
            aTMP.alignment = TextAlignmentOptions.Center;
            aTMP.color = textColor;
            aTMP.fontSize = 4f;
            aTMP.sortingOrder = sortingOrder + 1;

            GameObject goB = new("DieB");
            goB.transform.SetParent(transform, false);
            bSR = goB.AddComponent<SpriteRenderer>();
            bSR.sprite = RuntimeSpriteFactory.MakeSquareSprite(pixelSize, diceColor);
            bSR.sortingOrder = sortingOrder;
            goB.transform.localScale = Vector3.one * scale;
            GameObject lb = new("LabelB");
            lb.transform.SetParent(goB.transform, false);
            bTMP = lb.AddComponent<TextMeshPro>();
            bTMP.alignment = TextAlignmentOptions.Center;
            bTMP.color = textColor;
            bTMP.fontSize = 4f;
            bTMP.sortingOrder = sortingOrder + 1;

            UpdateFaces(0, 0);
        }

        private void LateUpdate()
        {
            if (driver == null) driver = FindObjectOfType<GameDriver>();
            Camera cam = Camera.main;
            if (cam == null) return;

            float y = cam.transform.position.y - cam.orthographicSize + edgePadding;
            float xCenter = cam.transform.position.x;
            transform.position = new Vector3(xCenter, y, 0f);

            Transform dieA = transform.Find("DieA");
            Transform dieB = transform.Find("DieB");
            if (dieA != null) dieA.localPosition = new Vector3(-horizontalGap * 0.5f, 0f, 0f);
            if (dieB != null) dieB.localPosition = new Vector3(+horizontalGap * 0.5f, 0f, 0f);

            if (driver != null && driver.Game != null)
            {
                int d1 = driver.Game.state.lastD1;
                int d2 = driver.Game.state.lastD2;
                if (d1 != shownA || d2 != shownB) UpdateFaces(d1, d2);
            }
        }

        private void UpdateFaces(int a, int b)
        {
            shownA = a;
            shownB = b;
            aTMP.text = a > 0 ? a.ToString() : "-";
            bTMP.text = b > 0 ? b.ToString() : "-";
            aTMP.rectTransform.sizeDelta = new Vector2(2f, 2f);
            bTMP.rectTransform.sizeDelta = new Vector2(2f, 2f);
        }
    }
}