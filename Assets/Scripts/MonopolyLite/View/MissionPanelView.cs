using MonopolyLite.Core;
using MonopolyLite.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class MissionPanelView : MonoBehaviour
    {
        GameController _controller;
        RectTransform _panel;
        TextMeshProUGUI _titleLabel;
        TextMeshProUGUI[] _missionLabels;

        static readonly Color PanelBg = new Color(0.05f, 0.05f, 0.15f, 0.85f);

        public void Initialize(GameController controller, RectTransform canvasRect)
        {
            _controller = controller;
            BuildPanel(canvasRect);
            controller.OnMissionCompleted += _ => Refresh();
            controller.OnAllMissionsCompleted += Refresh;
            controller.OnRollComplete += (_, _) => Refresh();
            Refresh();
        }

        void BuildPanel(RectTransform canvas)
        {
            var panelGo = new GameObject("MissionPanel", typeof(RectTransform));
            panelGo.transform.SetParent(canvas, false);
            _panel = panelGo.GetComponent<RectTransform>();
            _panel.anchorMin = new Vector2(0f, 0f);
            _panel.anchorMax = new Vector2(0f, 0f);
            _panel.anchoredPosition = new Vector2(20f, 20f);
            _panel.sizeDelta = new Vector2(320f, 200f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = PanelBg;

            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(_panel, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -15f);
            titleRt.sizeDelta = new Vector2(0f, 28f);
            _titleLabel = titleGo.AddComponent<TextMeshProUGUI>();
            _titleLabel.text = "DAILY MISSIONS";
            _titleLabel.fontSize = 18f;
            _titleLabel.fontStyle = FontStyles.Bold;
            _titleLabel.alignment = TextAlignmentOptions.Center;
            _titleLabel.color = Color.white;

            _missionLabels = new TextMeshProUGUI[5];
            for (int i = 0; i < 5; i++)
            {
                var labelGo = new GameObject($"Mission_{i}", typeof(RectTransform));
                labelGo.transform.SetParent(_panel, false);
                var labelRt = labelGo.GetComponent<RectTransform>();
                labelRt.anchorMin = new Vector2(0f, 1f);
                labelRt.anchorMax = new Vector2(1f, 1f);
                labelRt.anchoredPosition = new Vector2(0f, -42f - i * 28f);
                labelRt.sizeDelta = new Vector2(-20f, 26f);
                _missionLabels[i] = labelGo.AddComponent<TextMeshProUGUI>();
                _missionLabels[i].fontSize = 14f;
                _missionLabels[i].color = Color.white;
                _missionLabels[i].text = "";
            }
        }

        void Refresh()
        {
            var missions = _controller?.State?.MissionState?.Missions;
            if (missions == null) return;

            for (int i = 0; i < _missionLabels.Length; i++)
            {
                if (i < missions.Length)
                {
                    var m = missions[i];
                    string check = m.Completed ? "[X]" : "[ ]";
                    _missionLabels[i].text = $"{check} {m.Description} ({m.Progress}/{m.Target})";
                    _missionLabels[i].color = m.Completed ? Color.green : Color.white;
                }
                else
                {
                    _missionLabels[i].text = "";
                }
            }
        }
    }
}
