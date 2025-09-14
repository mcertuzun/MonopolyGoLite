using UnityEngine;
using UnityEngine.Rendering;

namespace MonopolyLite
{
    [RequireComponent(typeof(Camera))]
    public class MobileCamera2D : MonoBehaviour
    {
        public BoardView boardView;
        public float margin = 1.5f;
        public int targetFps = 60;

        private void Start()
        {
            Application.targetFrameRate = targetFps;

            Camera cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.transparencySortMode = TransparencySortMode.Orthographic;
            GraphicsSettings.transparencySortAxis = new Vector3(0, 0, 1);
            cam.cullingMask = ~0;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100f;

            if (boardView == null) boardView = FindObjectOfType<BoardView>();
            float half = (boardView != null ? boardView.sideLength * 0.5f : 6f) + margin;
            cam.orthographicSize = half;

            Vector3 pos = transform.position;
            pos.x = 0f;
            pos.y = 0f;
            pos.z = -10f; // <- önemli
            transform.position = pos;
        }
    }
}