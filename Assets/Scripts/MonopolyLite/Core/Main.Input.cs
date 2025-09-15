using UnityEngine;

namespace MonopolyLite
{
    public partial class Main
    {
        private void UpdateUIPositions()
        {
            Vector3 c = cam.transform.position;
            float bottom = c.y - cam.orthographicSize;
            rollBtn.position = new Vector3(c.x, Mathf.Lerp(bottom, c.y, 0.65f), 0);
            bool canRoll = diceCharges > 0 && jailTurns[currentPlayer] <= 0;
            rollBtnLabel.text = canRoll ? $"ROLL\n{diceCharges}" : $"WAIT\n{diceCharges}";
            rollBtnSR.color = canRoll ? config.rollColor : config.rollDisabled;
            multBtn.position = rollBtn.position + new Vector3(1.2f, 1.2f, 0);
            multBtnLabel.text = "x" + gainMultiplier;
            diceRoot.position = new Vector3(c.x, bottom + 0.6f, 0);
            diceRoot.Find("A").localPosition = new Vector3(-0.8f, 0, 0);
            diceRoot.Find("B").localPosition = new Vector3(+0.8f, 0, 0);
            Vector2 tap = Vector2.zero;
            bool pressed = false;
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
                tap = new Vector2(wp.x, wp.y);
                pressed = true;
            }
#else
            if(Input.touchCount>0 && Input.GetTouch(0).phase==TouchPhase.Began){ var wp = cam.ScreenToWorldPoint(Input.GetTouch(0).position); tap = new Vector2(wp.x,wp.y); pressed = true; }
#endif
            if (pressed)
            {
                if (Vector2.Distance(tap, new Vector2(rollBtn.position.x, rollBtn.position.y)) <= 1.6f * 0.9f)
                    TryRoll();
                else if (Vector2.Distance(tap, new Vector2(multBtn.position.x, multBtn.position.y)) <= 0.9f) gainMultiplier = gainMultiplier == 3 ? 1 : gainMultiplier + 1;
            }
        }
    }
}