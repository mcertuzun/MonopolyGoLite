using System.Collections.Generic;
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
            // Buy/Decline buttons
            if (pendingBuyTileIndex >= 0)
            {
                buyBtn.gameObject.SetActive(true);
                declineBtn.gameObject.SetActive(true);
                int price = GetPropertyPrice(pendingBuyTileIndex);
                bool isDiscount = declinedProperties.Contains(pendingBuyTileIndex);
                buyBtnLabel.text = $"BUY\n${price}" + (isDiscount ? "\n20% OFF" : "");
                declineBtnLabel.text = "PASS";
                buyBtn.position = new Vector3(c.x - 1.5f, Mathf.Lerp(bottom, c.y, 0.45f), 0);
                declineBtn.position = new Vector3(c.x + 1.5f, Mathf.Lerp(bottom, c.y, 0.45f), 0);
                buyBtnLabel.rectTransform.sizeDelta = new Vector2(4, 3);
                declineBtnLabel.rectTransform.sizeDelta = new Vector2(4, 3);
                buyBtn.GetComponent<SpriteRenderer>().sortingOrder = 100;
                declineBtn.GetComponent<SpriteRenderer>().sortingOrder = 100;
                buyBtnLabel.sortingOrder = 101;
                declineBtnLabel.sortingOrder = 101;
            }
            else
            {
                buyBtn.gameObject.SetActive(false);
                declineBtn.gameObject.SetActive(false);
            }

            // Build button
            bool canBuild = !gameOver && pendingBuyTileIndex < 0 && GetBuildableTiles(currentPlayer).Count > 0;
            buildBtn.gameObject.SetActive(canBuild);
            if (canBuild)
            {
                buildBtn.position = rollBtn.position + new Vector3(-1.2f, 1.2f, 0);
                buildBtnLabel.text = "BUILD";
                buildBtnLabel.rectTransform.sizeDelta = new Vector2(4, 2);
                buildBtn.GetComponent<SpriteRenderer>().sortingOrder = 100;
                buildBtnLabel.sortingOrder = 101;
            }

            // Card reveal
            if (cardRevealTimer > 0)
            {
                cardRevealTimer -= Time.deltaTime;
                cardRevealText.gameObject.SetActive(true);
                cardRevealText.text = lastCardDescription;
                cardRevealText.transform.position = c + new Vector3(0, 2f, 0);
                cardRevealText.sortingOrder = 150;
            }
            else
            {
                cardRevealText.gameObject.SetActive(false);
            }

            // Game over screen
            if (gameOver)
            {
                gameOverText.gameObject.SetActive(true);
                gameOverText.transform.position = c + new Vector3(0, 0, 0);
                gameOverText.sortingOrder = 200;
                int propsOwned = 0;
                int housesBuilt = 0;
                for (int i = 0; i < config.tiles.Length; i++)
                {
                    if (tileOwner[i] == 0)
                    {
                        propsOwned++;
                        housesBuilt += developmentLevels[i];
                    }
                }
                string result = playerWon ? "YOU WIN!" : "BANKRUPT!";
                gameOverText.text = $"{result}\n\nTurns: {totalTurns}\nMoney Earned: ${totalMoneyEarned}\nProperties: {propsOwned}\nDevelopment: {housesBuilt}";
                rollBtn.gameObject.SetActive(false);
                multBtn.gameObject.SetActive(false);
            }

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
                if (gameOver)
                {
                    // Ignore taps when game is over
                }
                else if (pendingBuyTileIndex >= 0)
                {
                    if (Vector2.Distance(tap, new Vector2(buyBtn.position.x, buyBtn.position.y)) <= 1.2f)
                    {
                        BuyProperty(currentPlayer, pendingBuyTileIndex);
                        pendingBuyTileIndex = -1;
                        EndTurn();
                    }
                    else if (Vector2.Distance(tap, new Vector2(declineBtn.position.x, declineBtn.position.y)) <= 1.2f)
                    {
                        DeclineProperty(pendingBuyTileIndex);
                        pendingBuyTileIndex = -1;
                        EndTurn();
                    }
                }
                else
                {
                    if (Vector2.Distance(tap, new Vector2(rollBtn.position.x, rollBtn.position.y)) <= 1.6f * 0.9f)
                        TryRoll();
                    else if (Vector2.Distance(tap, new Vector2(multBtn.position.x, multBtn.position.y)) <= 0.9f)
                        gainMultiplier = gainMultiplier == 3 ? 1 : gainMultiplier + 1;
                    else if (buildBtn.gameObject.activeSelf && Vector2.Distance(tap, new Vector2(buildBtn.position.x, buildBtn.position.y)) <= 1.0f)
                    {
                        List<int> buildable = GetBuildableTiles(currentPlayer);
                        if (buildable.Count > 0)
                            BuildHouse(currentPlayer, buildable[0]);
                    }
                }
            }
        }
    }
}