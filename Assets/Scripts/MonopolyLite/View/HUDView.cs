using MonopolyLite.Core;
using MonopolyLite.Data;
using MonopolyLite.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class HUDView : MonoBehaviour
    {
        GameController _controller;

        TextMeshProUGUI _diceLabel;
        TextMeshProUGUI _coinsLabel;
        TextMeshProUGUI _shieldsLabel;
        TextMeshProUGUI _networthLabel;
        TextMeshProUGUI _statusLabel;
        TextMeshProUGUI _regenLabel;
        TextMeshProUGUI _boardLabel;

        Button _rollButton;
        Button _multiplierButton;
        TextMeshProUGUI _multiplierLabel;

        int _multiplierIndex = 0;

        public void Initialize(GameController controller, RectTransform canvasRect)
        {
            _controller = controller;

            BuildStatsPanel(canvasRect);
            BuildStatusText(canvasRect);
            BuildRollButton(canvasRect);
            BuildMultiplierButton(canvasRect);

            controller.OnRollComplete += HandleRollComplete;
            controller.OnTileResolved += HandleTileResolved;
            controller.OnMilestonesReached += HandleMilestonesReached;
            controller.OnDiceRegenerated += HandleDiceRegenerated;
            controller.OnBoardTransition += HandleBoardTransition;
            controller.OnDailyRewardClaimed += HandleDailyReward;
            controller.OnHeistResolved += HandleHeistResolved;
            controller.OnShutdownStarted += HandleShutdownStarted;
            controller.OnShutdownResolved += HandleShutdownResolved;

            RefreshStats();
        }

        // ── Stats panel (top-left) ────────────────────────────────────────────

        void BuildStatsPanel(RectTransform canvas)
        {
            var panel = CreatePanel(canvas, "StatsPanel",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -20f), new Vector2(260f, 210f));

            var bg = panel.gameObject.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);

            _diceLabel     = CreateLabel(panel, "DiceLabel",     new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -10f),  new Vector2(240f, 28f));
            _coinsLabel    = CreateLabel(panel, "CoinsLabel",    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -42f),  new Vector2(240f, 28f));
            _shieldsLabel  = CreateLabel(panel, "ShieldsLabel",  new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -74f),  new Vector2(240f, 28f));
            _networthLabel = CreateLabel(panel, "NetWorthLabel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -106f), new Vector2(240f, 28f));
            _regenLabel = CreateLabel(panel, "RegenLabel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -138f), new Vector2(240f, 28f));
            _boardLabel = CreateLabel(panel, "BoardLabel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -170f), new Vector2(240f, 28f));
        }

        // ── Status text (center, above roll button) ───────────────────────────

        void BuildStatusText(RectTransform canvas)
        {
            var rt = CreatePanel(canvas, "StatusText",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 220f), new Vector2(600f, 50f));

            _statusLabel = rt.gameObject.AddComponent<TextMeshProUGUI>();
            _statusLabel.alignment = TextAlignmentOptions.Center;
            _statusLabel.fontSize = 24f;
            _statusLabel.color = Color.white;
            _statusLabel.text = "Roll the dice!";
        }

        // ── Roll button (center-bottom) ───────────────────────────────────────

        void BuildRollButton(RectTransform canvas)
        {
            var rt = CreatePanel(canvas, "RollButton",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-80f, 80f), new Vector2(200f, 70f));

            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.55f, 0.15f);

            _rollButton = rt.gameObject.AddComponent<Button>();

            var label = CreateLabel(rt, "Label",
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                Vector2.zero, Vector2.zero);
            label.text = "ROLL";
            label.fontSize = 28f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;

            // Stretch label to fill button
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;

            _rollButton.onClick.AddListener(() => { _controller.DoRoll(); RefreshStats(); });
        }

        // ── Multiplier button (right of roll button) ──────────────────────────

        void BuildMultiplierButton(RectTransform canvas)
        {
            var rt = CreatePanel(canvas, "MultiplierButton",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(140f, 80f), new Vector2(120f, 70f));

            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.25f, 0.65f);

            _multiplierButton = rt.gameObject.AddComponent<Button>();

            _multiplierLabel = CreateLabel(rt, "Label",
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            _multiplierLabel.text = "1x";
            _multiplierLabel.fontSize = 26f;
            _multiplierLabel.fontStyle = FontStyles.Bold;
            _multiplierLabel.alignment = TextAlignmentOptions.Center;
            _multiplierLabel.color = Color.white;

            _multiplierLabel.rectTransform.anchorMin = Vector2.zero;
            _multiplierLabel.rectTransform.anchorMax = Vector2.one;
            _multiplierLabel.rectTransform.offsetMin = Vector2.zero;
            _multiplierLabel.rectTransform.offsetMax = Vector2.zero;

            _multiplierButton.onClick.AddListener(CycleMultiplier);
        }

        // ── Event handlers ────────────────────────────────────────────────────

        void HandleRollComplete(RollResult roll, MoveResult move)
        {
            if (!roll.Success)
            {
                _statusLabel.text = "Not enough dice!";
                return;
            }

            string doublesText = roll.IsDoubles ? " (Doubles!)" : "";
            if (move.PassedGo)
                _statusLabel.text = $"Rolled {roll.Die1}+{roll.Die2}={roll.Total}{doublesText} — Passed GO!";
            else
                _statusLabel.text = $"Rolled {roll.Die1}+{roll.Die2}={roll.Total}{doublesText}";

            RefreshStats();
        }

        void HandleTileResolved(TileResolveResult result)
        {
            string extra = result.Type switch
            {
                TileResolveType.CoinsGained => $" +{result.Amount} coins",
                TileResolveType.CoinsLost   => $" -{result.Amount} coins",
                TileResolveType.Jail        => " -> JAIL",
                TileResolveType.Card        => result.DrawnCard.HasValue
                    ? $" Card: {result.DrawnCard.Value.description}"
                    : " Card drawn",
                TileResolveType.Railroad    => $" Railroad +{result.Amount}",
                _                           => ""
            };

            _statusLabel.text += extra;
            RefreshStats();
        }

        void HandleMilestonesReached(System.Collections.Generic.List<int> milestoneIndices)
        {
            _statusLabel.text = $"Milestone reached! ({milestoneIndices.Count} new)";
            _multiplierIndex = 0;
            var unlocked = _controller.GetUnlockedMultipliers();
            _multiplierLabel.text = $"{unlocked[0]}x";
            _controller.SetMultiplier(unlocked[0]);
            RefreshStats();
        }

        void HandleDiceRegenerated(int amount)
        {
            RefreshStats();
        }

        void HandleBoardTransition(string newBoardId)
        {
            _statusLabel.text = $"New board: {_controller.BoardDef.theme}!";
            RefreshStats();
        }

        void HandleDailyReward(MonopolyLite.Data.DailyRewardDef reward)
        {
            _statusLabel.text = $"Daily reward! +{reward.coins} coins, +{reward.dice} dice (Day {reward.day})";
            RefreshStats();
        }

        void HandleHeistResolved(HeistResult result, TargetProfile target)
        {
            if (result.IsMatch)
                _statusLabel.text = $"Heist vs {target.displayName}: {result.MatchedSymbol}! +{result.CoinsEarned}";
            else
                _statusLabel.text = $"Heist vs {target.displayName}: Miss! +{result.CoinsEarned}";
            RefreshStats();
        }

        void HandleShutdownStarted(TargetProfile target)
        {
            _statusLabel.text = $"Shutdown! Choose a landmark on {target.displayName}'s board...";
        }

        void HandleShutdownResolved(ShutdownResult result)
        {
            if (result.Shielded)
                _statusLabel.text = $"Shutdown blocked by shield! +{result.CoinsEarned}";
            else
                _statusLabel.text = $"SHUTDOWN on {result.TargetName}! +{result.CoinsEarned}";
            RefreshStats();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        void CycleMultiplier()
        {
            var unlocked = _controller.GetUnlockedMultipliers();
            if (unlocked.Count == 0) return;

            _multiplierIndex = (_multiplierIndex + 1) % unlocked.Count;
            int value = unlocked[_multiplierIndex];
            _controller.SetMultiplier(value);
            _multiplierLabel.text = $"{value}x";
        }

        public void RefreshStats()
        {
            if (_controller?.State == null) return;
            var p = _controller.State.Player;
            _diceLabel.text     = $"Dice: {p.Dice} / {p.DiceCap}";
            _coinsLabel.text    = $"Coins: {p.Coins}";
            _shieldsLabel.text  = $"Shields: {p.Shields}/3";
            _networthLabel.text = $"Net Worth: {p.NetWorth}";

            var prog = _controller.State.Progression;
            if (prog != null)
            {
                int regenSec = prog.DiceRegenSeconds;
                _regenLabel.text = $"Regen: 1 / {regenSec / 60}m{regenSec % 60:D2}s";
                _boardLabel.text = $"Board: {_controller.BoardDef.theme ?? "Unknown"}";
            }
            else
            {
                _regenLabel.text = "";
                _boardLabel.text = "";
            }
        }

        // ── UI factory helpers ────────────────────────────────────────────────

        static RectTransform CreatePanel(RectTransform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin       = anchorMin;
            rt.anchorMax       = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta       = sizeDelta;
            return rt;
        }

        static TextMeshProUGUI CreateLabel(RectTransform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = anchorMin;
            rt.anchorMax        = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = sizeDelta;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 22f;
            tmp.color    = Color.white;
            return tmp;
        }
    }
}
