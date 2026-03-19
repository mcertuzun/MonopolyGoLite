using MonopolyLite.Core;
using MonopolyLite.Data;
using MonopolyLite.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class LandmarkPanelView : MonoBehaviour
    {
        GameController _controller;
        RectTransform _listContainer;

        const float RowHeight = 60f;
        const float RowPad    = 4f;

        public void Initialize(GameController controller, RectTransform canvasRect)
        {
            _controller = controller;

            BuildPanel(canvasRect);

            controller.OnRollComplete     += (_, __) => Refresh();
            controller.OnLandmarkUpgraded += (_, __)  => Refresh();

            Refresh();
        }

        // ── Panel construction ────────────────────────────────────────────────

        void BuildPanel(RectTransform canvas)
        {
            // Outer panel — right side of screen
            var panel = CreateRect(canvas, "LandmarkPanel");
            panel.anchorMin        = new Vector2(1f, 0.5f);
            panel.anchorMax        = new Vector2(1f, 0.5f);
            panel.pivot            = new Vector2(1f, 0.5f);
            panel.anchoredPosition = new Vector2(-10f, 0f);
            panel.sizeDelta        = new Vector2(280f, 600f);

            var bg = panel.gameObject.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.65f);

            // Header
            var header = CreateRect(panel, "Header");
            header.anchorMin        = new Vector2(0f, 1f);
            header.anchorMax        = new Vector2(1f, 1f);
            header.pivot            = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta        = new Vector2(0f, 38f);

            var headerLabel = header.gameObject.AddComponent<TextMeshProUGUI>();
            headerLabel.text      = "LANDMARKS";
            headerLabel.fontSize  = 20f;
            headerLabel.fontStyle = FontStyles.Bold;
            headerLabel.alignment = TextAlignmentOptions.Center;
            headerLabel.color     = Color.white;

            // Scrollable list area
            var listArea = CreateRect(panel, "ListArea");
            listArea.anchorMin        = new Vector2(0f, 0f);
            listArea.anchorMax        = new Vector2(1f, 1f);
            listArea.anchoredPosition = new Vector2(0f, 0f);
            listArea.offsetMin        = new Vector2(0f, 0f);
            listArea.offsetMax        = new Vector2(0f, -38f);

            var scrollRect = listArea.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical   = true;

            // Viewport
            var viewport = CreateRect(listArea, "Viewport");
            viewport.anchorMin  = Vector2.zero;
            viewport.anchorMax  = Vector2.one;
            viewport.offsetMin  = Vector2.zero;
            viewport.offsetMax  = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();
            scrollRect.viewport = viewport;

            // Content container
            _listContainer = CreateRect(viewport, "Content");
            _listContainer.anchorMin  = new Vector2(0f, 1f);
            _listContainer.anchorMax  = new Vector2(1f, 1f);
            _listContainer.pivot      = new Vector2(0.5f, 1f);
            _listContainer.anchoredPosition = Vector2.zero;
            _listContainer.sizeDelta  = Vector2.zero;
            scrollRect.content = _listContainer;
        }

        // ── Refresh ───────────────────────────────────────────────────────────

        void Refresh()
        {
            if (_controller?.State == null) return;

            // Clear existing rows
            for (int i = _listContainer.childCount - 1; i >= 0; i--)
                Destroy(_listContainer.GetChild(i).gameObject);

            var landmarks = _controller.BoardDef.landmarks;
            float totalHeight = landmarks.Length * (RowHeight + RowPad);
            _listContainer.sizeDelta = new Vector2(0f, totalHeight);

            for (int i = 0; i < landmarks.Length; i++)
            {
                var lm = landmarks[i];
                float yPos = -i * (RowHeight + RowPad) - RowPad;
                BuildRow(_listContainer, lm, yPos);
            }
        }

        void BuildRow(RectTransform parent, LandmarkDef lm, float yPos)
        {
            int level = _controller.State.Board.GetLandmarkLevel(lm.colorGroup);
            bool maxed = level >= 5;
            bool canAfford = _controller.CanUpgradeLandmark(lm.colorGroup);
            int cost = _controller.GetUpgradeCost(lm.colorGroup);

            var row = CreateRect(parent, $"Row_{lm.colorGroup}");
            row.anchorMin        = new Vector2(0f, 1f);
            row.anchorMax        = new Vector2(1f, 1f);
            row.pivot            = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0f, yPos);
            row.sizeDelta        = new Vector2(0f, RowHeight);

            var rowBg = row.gameObject.AddComponent<Image>();
            rowBg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Color swatch (left edge)
            var swatch = CreateRect(row, "Swatch");
            swatch.anchorMin        = new Vector2(0f, 0f);
            swatch.anchorMax        = new Vector2(0f, 1f);
            swatch.pivot            = new Vector2(0f, 0.5f);
            swatch.anchoredPosition = Vector2.zero;
            swatch.sizeDelta        = new Vector2(10f, 0f);
            var swatchImg = swatch.gameObject.AddComponent<Image>();
            swatchImg.color = GetGroupColor(lm.colorGroup);

            // Name + level label
            var nameArea = CreateRect(row, "NameArea");
            nameArea.anchorMin        = new Vector2(0f, 0f);
            nameArea.anchorMax        = new Vector2(0f, 1f);
            nameArea.pivot            = new Vector2(0f, 0.5f);
            nameArea.anchoredPosition = new Vector2(14f, 0f);
            nameArea.sizeDelta        = new Vector2(150f, 0f);

            var nameLabel = nameArea.gameObject.AddComponent<TextMeshProUGUI>();
            nameLabel.text      = $"{lm.name}\nL{level}/5";
            nameLabel.fontSize  = 14f;
            nameLabel.color     = maxed ? new Color(0.3f, 1f, 0.3f) : Color.white;
            nameLabel.alignment = TextAlignmentOptions.Left;

            // Build button
            var btnRect = CreateRect(row, "BuildBtn");
            btnRect.anchorMin        = new Vector2(1f, 0.5f);
            btnRect.anchorMax        = new Vector2(1f, 0.5f);
            btnRect.pivot            = new Vector2(1f, 0.5f);
            btnRect.anchoredPosition = new Vector2(-6f, 0f);
            btnRect.sizeDelta        = new Vector2(96f, 44f);

            var btnImg = btnRect.gameObject.AddComponent<Image>();
            btnImg.color = maxed ? new Color(0.3f, 0.3f, 0.3f) :
                           canAfford ? new Color(0.15f, 0.45f, 0.15f) :
                                       new Color(0.25f, 0.25f, 0.25f);

            var btn = btnRect.gameObject.AddComponent<Button>();
            btn.interactable = !maxed && canAfford;

            var btnLabel = CreateLabel(btnRect, "BtnLabel");
            btnLabel.rectTransform.anchorMin  = Vector2.zero;
            btnLabel.rectTransform.anchorMax  = Vector2.one;
            btnLabel.rectTransform.offsetMin  = Vector2.zero;
            btnLabel.rectTransform.offsetMax  = Vector2.zero;
            btnLabel.text      = maxed ? "MAX" : $"{cost}c";
            btnLabel.fontSize  = 14f;
            btnLabel.fontStyle = FontStyles.Bold;
            btnLabel.alignment = TextAlignmentOptions.Center;
            btnLabel.color     = maxed || !canAfford ? new Color(0.55f, 0.55f, 0.55f) : Color.white;

            if (!maxed)
            {
                var capturedGroup = lm.colorGroup;
                btn.onClick.AddListener(() =>
                {
                    _controller.DoUpgradeLandmark(capturedGroup);
                    Refresh();
                });
            }
        }

        // ── UI factory helpers ────────────────────────────────────────────────

        static RectTransform CreateRect(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        static TextMeshProUGUI CreateLabel(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<TextMeshProUGUI>();
            return go.GetComponent<TextMeshProUGUI>();
        }

        static Color GetGroupColor(ColorGroup group) => group switch
        {
            ColorGroup.Brown     => new Color(0.55f, 0.27f, 0.07f),
            ColorGroup.LightBlue => new Color(0.68f, 0.85f, 0.90f),
            ColorGroup.Pink      => new Color(0.85f, 0.44f, 0.84f),
            ColorGroup.Orange    => new Color(1.0f,  0.65f, 0.0f),
            ColorGroup.Red       => new Color(0.9f,  0.1f,  0.1f),
            ColorGroup.Yellow    => new Color(1.0f,  0.95f, 0.0f),
            ColorGroup.Green     => new Color(0.0f,  0.7f,  0.0f),
            ColorGroup.Blue      => new Color(0.0f,  0.0f,  0.8f),
            _                    => Color.grey
        };
    }
}
