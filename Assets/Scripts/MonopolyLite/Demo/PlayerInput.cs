using UnityEngine;
using UnityEngine.EventSystems;

namespace MonopolyLite
{
    public class PlayerInput : MonoBehaviour
    {
        public GameDriver driver;
        public int playerIndex = 0;

        public int[] botIndices = new int[]
        { 1 };

        private void Update()
        {
            if (driver == null) driver = FindObjectOfType<GameDriver>();
            if (driver == null || driver.Game == null) return;
            if (driver.mode != GameMode.Player) return;
            int p = driver.Game.state.currentPlayer;
            if (p != playerIndex) return;
            if (IsBot(p)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            bool pressed = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
            if (!pressed) return;
            if (driver.Game.state.jailTurns[p] > 0) return;
            driver.Game.Enqueue(Command.Roll(p));
        }

        private bool IsBot(int i)
        {
            for (int k = 0; k < botIndices.Length; k++)
                if (botIndices[k] == i)
                    return true;
            return false;
        }
    }
}