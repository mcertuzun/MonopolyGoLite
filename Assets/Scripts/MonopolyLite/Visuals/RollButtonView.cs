using TMPro;
using UnityEngine;

namespace MonopolyLite
{
    public class RollButtonView : MonoBehaviour
    {
        public GameDriver driver;
        public float radius = 1.6f;
        public int pixel = 128;
        public Color baseColor = new(0.15f, 0.8f, 0.35f);
        public Color disabledColor = new(0.35f, 0.35f, 0.35f);
        public int sorting = 200;
        private Camera cam;
        private TextMeshPro label;

        private SpriteRenderer sr;

        private void Start()
        {
            cam = Camera.main;
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.MakeCircleSprite(pixel, baseColor);
            sr.sortingOrder = sorting;
            transform.localScale = Vector3.one * radius;

            GameObject tgo = new("Label");
            tgo.transform.SetParent(transform, false);
            label = tgo.AddComponent<TextMeshPro>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 4f;
            label.sortingOrder = sorting + 1;
        }

        private void Update()
        {
            if (driver == null) driver = FindObjectOfType<GameDriver>();
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            Vector3 c = cam.transform.position;
            float y = c.y - cam.orthographicSize * 0.35f;
            transform.position = new Vector3(c.x, y, 0f);

            int charges = driver != null && driver.Game != null ? driver.Game.state.diceCharges : 0;
            int mult = driver != null && driver.Game != null ? driver.Game.state.gainMultiplier : 1;
            bool enabled = charges > 0 && driver.Game.state.jailTurns[driver.Game.state.currentPlayer] <= 0;
            sr.color = enabled ? baseColor : disabledColor;
            label.text = enabled ? $"ROLL\n{charges}" : $"WAIT\n{charges}";

            if (enabled && (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
            {
                Vector3 wp = Input.mousePresent ? cam.ScreenToWorldPoint(Input.mousePosition) : cam.ScreenToWorldPoint(Input.GetTouch(0).position);
                Vector2 p = new(wp.x, wp.y);
                if (Vector2.Distance(p, new Vector2(transform.position.x, transform.position.y)) <= radius * 0.9f)
                {
                    int cur = driver.Game.state.currentPlayer;
                    driver.Game.Enqueue(Command.Roll(cur));
                }
            }
        }
    }
}