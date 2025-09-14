using UnityEngine;
using UnityEngine.Rendering;

namespace MonopolyLite
{
    [RequireComponent(typeof(Camera))]
    public class CameraPortraitFit : MonoBehaviour
    {
        public BoardView boardView;
        public int targetWidth = 1080;
        public int targetHeight = 1920;
        public float marginWorld = 1.0f;

        private void Start()
        {
            Camera cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.transparencySortMode = TransparencySortMode.Orthographic;
            GraphicsSettings.transparencySortAxis = new Vector3(0, 0, 1);
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100f;

            if (boardView == null) boardView = FindObjectOfType<BoardView>();
            float side = boardView != null ? boardView.sideLength : 12f;
            float half = side * 0.5f + marginWorld;

            float aspect = (float)targetWidth / (float)targetHeight;
            float sizeByHeight = half;
            float sizeByWidth = half / Mathf.Max(0.0001f, aspect);
            cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);

            Vector3 c = boardView != null ? boardView.transform.position : Vector3.zero;
            transform.position = new Vector3(c.x, c.y, -10f);
            transform.rotation = Quaternion.identity;
        }
    }
}