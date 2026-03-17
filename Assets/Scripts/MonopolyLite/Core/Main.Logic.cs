using UnityEngine;

namespace MonopolyLite
{
    public partial class Main
    {
        private void TickSystems()
        {
            diceChargeTimer += 1f / Mathf.Max(1, config.ticksPerSecond);
            if (diceChargeTimer >= diceChargeInterval)
            {
                diceChargeTimer -= diceChargeInterval;
                if (diceCharges < diceChargeCap) diceCharges++;
            }
        }

        private void TryRoll()
        {
            if (diceCharges <= 0) return;
            if (jailTurns[currentPlayer] > 0)
            {
                jailTurns[currentPlayer]--;
                EndTurn();
                return;
            }

            diceCharges--;
            int d1 = rng.Next(1, 7), d2 = rng.Next(1, 7);
            lastD1 = d1;
            lastD2 = d2;
            int steps = d1 + d2;
            bool dbl = d1 == d2;
            int prev = pos[currentPlayer];
            int next = (prev + steps) % config.tiles.Length;
            bool passed = prev + steps >= config.tiles.Length;
            pos[currentPlayer] = next;
            if (passed) cash[currentPlayer] += config.goPayout * gainMultiplier;
            ResolveLanding(currentPlayer);
            if (dbl)
            {
                doublesInRow[currentPlayer]++;
                if (doublesInRow[currentPlayer] >= 3)
                {
                    SendToJail(currentPlayer);
                    EndTurn();
                }
            }
            else
            {
                doublesInRow[currentPlayer] = 0;
                EndTurn();
            }

            UpdateDiceLabels(d1, d2);
        }

        private void ResolveLanding(int p)
        {
            TileDef t = config.tiles[pos[p]];
            switch (t.type)
            {
                case TileType.Property:
                    int o = tileOwner[pos[p]];
                    if (o == -1)
                    {
                        if (cash[p] >= t.price)
                        {
                            cash[p] -= t.price;
                            tileOwner[pos[p]] = p;
                        }
                    }
                    else if (o != p)
                    {
                        cash[p] -= t.baseRent;
                        cash[o] += t.baseRent;
                    }

                    break;
                case TileType.Tax: cash[p] -= t.taxAmount; break;
                case TileType.Chance:
                case TileType.CommunityChest:
                    int delta = rng.Next(-50, 101);
                    if (delta > 0) delta *= gainMultiplier;
                    cash[p] += delta;
                    break;
                case TileType.GoToJail: SendToJail(p); break;
            }
        }

        private void EndTurn()
        {
            currentPlayer = (currentPlayer + 1) % playerCount;
        }

        private void SendToJail(int p)
        {
            pos[p] = Mathf.Clamp(config.jailTileIndex, 0, config.tiles.Length - 1);
            jailTurns[p] = 3;
            doublesInRow[p] = 0;
        }

        private void UpdateTokens()
        {
            if (worldTilePositions == null || worldTilePositions.Length != config.tiles.Length)
            {
                Vector3[] posArr = Layout.Perimeter(config.tiles.Length, config.sideLength, config.tileSize, 0.1f);
                localTilePositions = posArr;
                worldTilePositions = new Vector3[posArr.Length];
                for (int i = 0; i < posArr.Length; i++) worldTilePositions[i] = boardRoot.TransformPoint(posArr[i]);
            }

            int[] counts = new int[config.tiles.Length];
            for (int i = 0; i < playerCount; i++) counts[pos[i]]++;
            int[] seen = new int[config.tiles.Length];
            for (int p = 0; p < playerCount; p++)
            {
                int tile = pos[p];
                int rank = seen[tile]++;
                Vector3 basePos = worldTilePositions[tile];
                Vector3 off = Vector3.zero;
                if (counts[tile] > 1)
                {
                    float a = rank / Mathf.Max(1f, counts[tile]) * Mathf.PI * 2f;
                    off = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * (0.4f * config.tileSize);
                }

                Transform tr = tokenSprites[p].transform;
                tr.position = Vector3.Lerp(tr.position, basePos + off, Time.deltaTime * 14f);
            }
        }

        private void UpdateDiceLabels(int a, int b)
        {
            diceATxt.text = a > 0 ? a.ToString() : "-";
            diceBTxt.text = b > 0 ? b.ToString() : "-";
            diceATxt.rectTransform.sizeDelta = new Vector2(2, 2);
            diceBTxt.rectTransform.sizeDelta = new Vector2(2, 2);
        }

        private void UpdateStats()
        {
            Vector3 c = cam.transform.position;
            Vector3 head = c + new Vector3(-6, 7, 0);
            for (int i = 0; i < statsLines.Length; i++)
            {
                statsLines[i].transform.position = head + new Vector3(0, -i * 0.8f, 0);
                statsLines[i].sortingOrder = 120 + i;
            }

            statsLines[0].text = $"Turn P{currentPlayer} Dice:{lastD1}+{lastD2} x{gainMultiplier}";
            statsLines[1].text = $"Charges:{diceCharges}/{diceChargeCap}";
            for (int p = 0; p < playerCount; p++)
            {
                int tile = pos[p];
                statsLines[2 + p].text = $"P{p} ${cash[p]} T{tile} {config.tiles[tile].name}";
            }
        }

        private Color ColorForTile(TileDef tile)
        {
            switch (tile.type)
            {
                case TileType.Property:
                    return ColorForGroup(tile.colorGroup);
                case TileType.Tax: return config.taxColor;
                case TileType.Chance: return config.chanceColor;
                case TileType.CommunityChest: return config.communityChestColor;
                case TileType.Go: return config.goColor;
                case TileType.Jail: return config.jailColor;
                case TileType.GoToJail: return config.gotoJailColor;
                case TileType.Railroad: return config.railroadColor;
                case TileType.Utility: return config.utilityColor;
                default: return Color.white;
            }
        }

        private Color ColorForGroup(ColorGroup g)
        {
            switch (g)
            {
                case ColorGroup.Brown: return config.brownGroup;
                case ColorGroup.LightBlue: return config.lightBlueGroup;
                case ColorGroup.Pink: return config.pinkGroup;
                case ColorGroup.Orange: return config.orangeGroup;
                case ColorGroup.Red: return config.redGroup;
                case ColorGroup.Yellow: return config.yellowGroup;
                case ColorGroup.Green: return config.greenGroup;
                case ColorGroup.Blue: return config.blueGroup;
                default: return config.propertyColor;
            }
        }
    }
}