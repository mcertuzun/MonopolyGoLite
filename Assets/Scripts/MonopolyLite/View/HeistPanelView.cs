using MonopolyLite.Core;
using MonopolyLite.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class HeistPanelView : MonoBehaviour
    {
        GameController _controller;
        RectTransform _panel;
        TextMeshProUGUI _titleLabel;
        TextMeshProUGUI _resultLabel;
        Image[] _gridCells;
        TextMeshProUGUI[] _gridLabels;

        static readonly Color CoinBagColor = new Color(0.9f, 0.75f, 0.2f);
        static readonly Color GoldBarColor = new Color(1f, 0.85f, 0f);
        static readonly Color DiamondColor = new Color(0.4f, 0.8f, 1f);
        static readonly Color PanelBg = new Color(0f, 0f, 0f, 0.85f);

        public void Initialize(GameController controller, RectTransform canvasRect)
        {
            _controller = controller;
            BuildPanel(canvasRect);
            Hide();
            controller.OnHeistResolved += HandleHeistResolved;
        }

        void BuildPanel(RectTransform canvas)
        {
            var panelGo = new GameObject("HeistPanel", typeof(RectTransform));
            panelGo.transform.SetParent(canvas, false);
            _panel = panelGo.GetComponent<RectTransform>();
            _panel.anchorMin = new Vector2(0.5f, 0.5f);
            _panel.anchorMax = new Vector2(0.5f, 0.5f);
            _panel.anchoredPosition = Vector2.zero;
            _panel.sizeDelta = new Vector2(500f, 450f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = PanelBg;

            // Title
            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(_panel, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -20f);
            titleRt.sizeDelta = new Vector2(0f, 40f);
            _titleLabel = titleGo.AddComponent<TextMeshProUGUI>();
            _titleLabel.text = "BANK HEIST";
            _titleLabel.fontSize = 28f;
            _titleLabel.fontStyle = FontStyles.Bold;
            _titleLabel.alignment = TextAlignmentOptions.Center;
            _titleLabel.color = Color.white;

            // Grid (3 rows x 4 cols = 12 cells)
            _gridCells = new Image[12];
            _gridLabels = new TextMeshProUGUI[12];
            float cellSize = 90f;
            float gridStartX = -180f;
            float gridStartY = -80f;

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int idx = row * 4 + col;
                    float x = gridStartX + col * (cellSize + 10f);
                    float y = gridStartY - row * (cellSize + 10f);

                    var cellGo = new GameObject($"Cell_{idx}", typeof(RectTransform));
                    cellGo.transform.SetParent(_panel, false);
                    var cellRt = cellGo.GetComponent<RectTransform>();
                    cellRt.anchorMin = new Vector2(0.5f, 1f);
                    cellRt.anchorMax = new Vector2(0.5f, 1f);
                    cellRt.anchoredPosition = new Vector2(x, y);
                    cellRt.sizeDelta = new Vector2(cellSize, cellSize);

                    _gridCells[idx] = cellGo.AddComponent<Image>();
                    _gridCells[idx].color = Color.gray;

                    var labelGo = new GameObject("Label", typeof(RectTransform));
                    labelGo.transform.SetParent(cellRt, false);
                    var labelRt = labelGo.GetComponent<RectTransform>();
                    labelRt.anchorMin = Vector2.zero;
                    labelRt.anchorMax = Vector2.one;
                    labelRt.offsetMin = Vector2.zero;
                    labelRt.offsetMax = Vector2.zero;

                    _gridLabels[idx] = labelGo.AddComponent<TextMeshProUGUI>();
                    _gridLabels[idx].fontSize = 16f;
                    _gridLabels[idx].alignment = TextAlignmentOptions.Center;
                    _gridLabels[idx].color = Color.white;
                }
            }

            // Result label
            var resultGo = new GameObject("Result", typeof(RectTransform));
            resultGo.transform.SetParent(_panel, false);
            var resultRt = resultGo.GetComponent<RectTransform>();
            resultRt.anchorMin = new Vector2(0f, 0f);
            resultRt.anchorMax = new Vector2(1f, 0f);
            resultRt.anchoredPosition = new Vector2(0f, 40f);
            resultRt.sizeDelta = new Vector2(0f, 50f);
            _resultLabel = resultGo.AddComponent<TextMeshProUGUI>();
            _resultLabel.fontSize = 22f;
            _resultLabel.alignment = TextAlignmentOptions.Center;
            _resultLabel.color = Color.yellow;
        }

        void HandleHeistResolved(HeistResult result, TargetProfile target)
        {
            _titleLabel.text = $"BANK HEIST vs {target.displayName}";

            for (int i = 0; i < 12; i++)
            {
                var symbol = result.Grid[i];
                _gridCells[i].color = GetSymbolColor(symbol);
                _gridLabels[i].text = GetSymbolText(symbol);
            }

            if (result.IsMatch)
                _resultLabel.text = $"Matched {result.MatchedSymbol}! +{result.CoinsEarned} coins";
            else
                _resultLabel.text = $"No match. +{result.CoinsEarned} coins";

            Show();
            Invoke(nameof(Hide), 3f);
        }

        static Color GetSymbolColor(HeistSymbol symbol) => symbol switch
        {
            HeistSymbol.CoinBag => CoinBagColor,
            HeistSymbol.GoldBar => GoldBarColor,
            HeistSymbol.Diamond => DiamondColor,
            _ => Color.gray,
        };

        static string GetSymbolText(HeistSymbol symbol) => symbol switch
        {
            HeistSymbol.CoinBag => "COIN",
            HeistSymbol.GoldBar => "GOLD",
            HeistSymbol.Diamond => "GEM",
            _ => "?",
        };

        void Show() { _panel.gameObject.SetActive(true); }
        public void Hide() { _panel.gameObject.SetActive(false); }
    }
}
