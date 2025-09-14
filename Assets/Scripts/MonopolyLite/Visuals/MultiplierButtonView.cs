using TMPro;
using UnityEngine;

namespace MonopolyLite
{
    public class MultiplierButtonView : MonoBehaviour
    {
        public RollButtonView anchor;
        public GameDriver driver;
        public float size = 0.9f;
        public Vector2 offset = new(1.2f, 1.2f);
        public int pixel = 96;
        public Color color = new(0.15f, 0.35f, 0.9f);
        public int sorting = 210;
        private Camera cam;
        private TextMeshPro label;

        private SpriteRenderer sr;

        private void Start()
        {
            cam = Camera.main;
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.MakeSquareSprite(pixel, color);
            sr.sortingOrder = sorting;
            transform.localScale = Vector3.one * size;

            GameObject tgo = new("Label");
            tgo.transform.SetParent(transform, false);
            label = tgo.AddComponent<TextMeshPro>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 3.5f;
            label.sortingOrder = sorting + 1;
        }

        private void Update()
        {
            if (driver == null) driver = FindObjectOfType<GameDriver>();
            if (cam == null) cam = Camera.main;
            if (anchor == null) anchor = FindObjectOfType<RollButtonView>();
            if (anchor == null) return;

            transform.position = anchor.transform.position + (Vector3)offset;

            int mult = driver != null && driver.Game != null ? driver.Game.state.gainMultiplier : 1;
            label.text = $"x{mult}";

            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                Vector3 wp = Input.mousePresent ? cam.ScreenToWorldPoint(Input.mousePosition) : cam.ScreenToWorldPoint(Input.GetTouch(0).position);
                Vector2 p = new(wp.x, wp.y);
                Vector2 c = new(transform.position.x, transform.position.y);
                if (Vector2.Distance(p, c) <= size)
                {
                    int next = mult == 3 ? 1 : mult + 1;
                    driver.Game.Enqueue(Command.SetMult(next));
                }
            }
        }
    }
}