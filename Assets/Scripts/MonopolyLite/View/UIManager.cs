using MonopolyLite.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MonopolyLite.View
{
    public class UIManager : MonoBehaviour
    {
        HUDView _hudView;
        LandmarkPanelView _landmarkPanelView;

        public void Initialize(GameController controller)
        {
            var canvasRect = BuildCanvas();
            EnsureEventSystem();

            var hudGo = new GameObject("HUDView");
            hudGo.transform.SetParent(transform, false);
            _hudView = hudGo.AddComponent<HUDView>();
            _hudView.Initialize(controller, canvasRect);

            var landmarkGo = new GameObject("LandmarkPanelView");
            landmarkGo.transform.SetParent(transform, false);
            _landmarkPanelView = landmarkGo.AddComponent<LandmarkPanelView>();
            _landmarkPanelView.Initialize(controller, canvasRect);

            controller.OnBoardComplete += HandleBoardComplete;
        }

        // ── Canvas ────────────────────────────────────────────────────────────

        RectTransform BuildCanvas()
        {
            var go = new GameObject("Canvas");
            go.transform.SetParent(transform, false);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            go.AddComponent<GraphicRaycaster>();

            return go.GetComponent<RectTransform>();
        }

        // ── EventSystem ───────────────────────────────────────────────────────

        static void EnsureEventSystem()
        {
#pragma warning disable CS0618
            if (Object.FindObjectOfType<EventSystem>() != null) return;
#pragma warning restore CS0618

            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        // ── Board complete ────────────────────────────────────────────────────

        void HandleBoardComplete()
        {
            Debug.Log("[UIManager] Board complete! All landmarks at level 5.");
        }
    }
}
