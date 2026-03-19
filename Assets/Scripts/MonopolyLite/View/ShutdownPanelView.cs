using MonopolyLite.Core;
using MonopolyLite.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class ShutdownPanelView : MonoBehaviour
    {
        GameController _controller;
        RectTransform _panel;
        TextMeshProUGUI _titleLabel;
        TextMeshProUGUI _shieldLabel;
        TextMeshProUGUI _resultLabel;
        RectTransform _landmarkContainer;
        bool _showingResult;

        static readonly Color PanelBg = new Color(0.15f, 0f, 0f, 0.9f);

        public void Initialize(GameController controller, RectTransform canvasRect)
        {
            _controller = controller;
            BuildPanel(canvasRect);
            Hide();
            controller.OnShutdownStarted += HandleShutdownStarted;
            controller.OnShutdownResolved += HandleShutdownResolved;
        }

        void BuildPanel(RectTransform canvas)
        {
            var panelGo = new GameObject("ShutdownPanel", typeof(RectTransform));
            panelGo.transform.SetParent(canvas, false);
            _panel = panelGo.GetComponent<RectTransform>();
            _panel.anchorMin = new Vector2(0.5f, 0.5f);
            _panel.anchorMax = new Vector2(0.5f, 0.5f);
            _panel.anchoredPosition = Vector2.zero;
            _panel.sizeDelta = new Vector2(500f, 500f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = PanelBg;

            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(_panel, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -20f);
            titleRt.sizeDelta = new Vector2(0f, 40f);
            _titleLabel = titleGo.AddComponent<TextMeshProUGUI>();
            _titleLabel.fontSize = 26f;
            _titleLabel.fontStyle = FontStyles.Bold;
            _titleLabel.alignment = TextAlignmentOptions.Center;
            _titleLabel.color = Color.white;

            var shieldGo = new GameObject("ShieldInfo", typeof(RectTransform));
            shieldGo.transform.SetParent(_panel, false);
            var shieldRt = shieldGo.GetComponent<RectTransform>();
            shieldRt.anchorMin = new Vector2(0f, 1f);
            shieldRt.anchorMax = new Vector2(1f, 1f);
            shieldRt.anchoredPosition = new Vector2(0f, -55f);
            shieldRt.sizeDelta = new Vector2(0f, 28f);
            _shieldLabel = shieldGo.AddComponent<TextMeshProUGUI>();
            _shieldLabel.fontSize = 20f;
            _shieldLabel.alignment = TextAlignmentOptions.Center;
            _shieldLabel.color = new Color(0.7f, 0.7f, 0.7f);

            var containerGo = new GameObject("LandmarkContainer", typeof(RectTransform));
            containerGo.transform.SetParent(_panel, false);
            _landmarkContainer = containerGo.GetComponent<RectTransform>();
            _landmarkContainer.anchorMin = new Vector2(0f, 0.15f);
            _landmarkContainer.anchorMax = new Vector2(1f, 0.85f);
            _landmarkContainer.offsetMin = new Vector2(20f, 0f);
            _landmarkContainer.offsetMax = new Vector2(-20f, -80f);

            var resultGo = new GameObject("Result", typeof(RectTransform));
            resultGo.transform.SetParent(_panel, false);
            var resultRt = resultGo.GetComponent<RectTransform>();
            resultRt.anchorMin = new Vector2(0f, 0f);
            resultRt.anchorMax = new Vector2(1f, 0f);
            resultRt.anchoredPosition = new Vector2(0f, 30f);
            resultRt.sizeDelta = new Vector2(0f, 50f);
            _resultLabel = resultGo.AddComponent<TextMeshProUGUI>();
            _resultLabel.fontSize = 22f;
            _resultLabel.alignment = TextAlignmentOptions.Center;
            _resultLabel.color = Color.red;
            _resultLabel.text = "";
        }

        void HandleShutdownStarted(TargetProfile target)
        {
            _showingResult = false;
            _titleLabel.text = $"SHUTDOWN: {target.displayName}";
            _shieldLabel.text = $"Shields: {target.shields}/3 | NW: {target.netWorth}";
            _resultLabel.text = "Pick a landmark to attack!";
            _resultLabel.color = Color.white;

            foreach (Transform child in _landmarkContainer)
                Destroy(child.gameObject);

            float yPos = 0f;
            foreach (var lm in target.landmarks)
            {
                BuildLandmarkButton(lm, yPos);
                yPos -= 48f;
            }

            Show();
        }

        void BuildLandmarkButton(TargetLandmark lm, float yPos)
        {
            var go = new GameObject($"LM_{lm.colorGroup}", typeof(RectTransform));
            go.transform.SetParent(_landmarkContainer, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(0f, yPos);
            rt.sizeDelta = new Vector2(0f, 44f);

            var btnBg = go.AddComponent<Image>();
            btnBg.color = new Color(0.3f, 0.1f, 0.1f);

            var btn = go.AddComponent<Button>();
            var group = lm.colorGroup;
            btn.onClick.AddListener(() =>
            {
                if (_showingResult) return;
                _controller.DoShutdownAttack(group);
            });

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(rt, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(10f, 0f);
            labelRt.offsetMax = new Vector2(-10f, 0f);

            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = $"{lm.name} (L{lm.level}/5) — {lm.colorGroup}";
            label.fontSize = 18f;
            label.alignment = TextAlignmentOptions.Left;
            label.color = Color.white;
        }

        void HandleShutdownResolved(ShutdownResult result)
        {
            _showingResult = true;
            if (result.Shielded)
            {
                _resultLabel.color = Color.cyan;
                _resultLabel.text = $"Blocked by shield! +{result.CoinsEarned} coins";
            }
            else
            {
                _resultLabel.color = Color.red;
                _resultLabel.text = $"SHUTDOWN! +{result.CoinsEarned} coins";
            }
            Invoke(nameof(Hide), 3f);
        }

        void Show() { _panel.gameObject.SetActive(true); }
        public void Hide() { _panel.gameObject.SetActive(false); }
    }
}
