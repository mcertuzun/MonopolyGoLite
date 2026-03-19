using MonopolyLite.View;
using UnityEngine;

namespace MonopolyLite.Core
{
    public class Bootstrap : MonoBehaviour
    {
        void Awake()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera", typeof(Camera));
                camGo.tag = "MainCamera";
                cam = camGo.GetComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 12f;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            cam.transform.position = Vector3.back * 10;
            var controllerGo = new GameObject("GameController");
            var controller = controllerGo.AddComponent<GameController>();
            controller.Initialize();

            var boardGo = new GameObject("Board");
            var boardRenderer = boardGo.AddComponent<BoardRenderer>();
            boardRenderer.Render(controller.BoardDef);

            var tokenGo = new GameObject("Token");
            var tokenRenderer = tokenGo.AddComponent<TokenRenderer>();
            tokenRenderer.Initialize(Color.white, controller.BoardDef.tileSize);
            tokenRenderer.MoveTo(boardRenderer.GetTilePosition(0));

            controller.OnRollComplete += (roll, move) =>
            {
                if (roll.Success)
                    tokenRenderer.MoveTo(boardRenderer.GetTilePosition(controller.State.Player.Position));
            };

            var uiGo = new GameObject("UI");
            var uiManager = uiGo.AddComponent<UIManager>();
            uiManager.Initialize(controller);
        }
    }
}
