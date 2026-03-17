using System.Collections.Generic;
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
            if (pendingBuyTileIndex >= 0) return;
            if (gameOver) return;
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
            if (passed) totalMoneyEarned += config.goPayout * gainMultiplier;
            ResolveLanding(currentPlayer);
            bool movedByCard = pos[currentPlayer] != next;
            if (dbl && !movedByCard)
            {
                doublesInRow[currentPlayer]++;
                if (doublesInRow[currentPlayer] >= 3)
                {
                    SendToJail(currentPlayer);
                    if (pendingBuyTileIndex < 0) EndTurn();
                }
            }
            else
            {
                doublesInRow[currentPlayer] = 0;
                if (pendingBuyTileIndex < 0) EndTurn();
            }

            UpdateDiceLabels(d1, d2);
        }

        private void ResolveLanding(int p)
        {
            TileDef t = config.tiles[pos[p]];
            switch (t.type)
            {
                case TileType.Property:
                {
                    int o = tileOwner[pos[p]];
                    if (o == -1)
                    {
                        int price = GetPropertyPrice(pos[p]);
                        if (cash[p] >= price)
                            pendingBuyTileIndex = pos[p];
                    }
                    else if (o != p)
                    {
                        int rent = GetPropertyRent(pos[p]);
                        if (OwnsFullGroup(o, t.colorGroup) && developmentLevels[pos[p]] == 0)
                            rent *= 2;
                        if (!TryDeductCash(p, rent)) break;
                        cash[o] += rent;
                    }
                    break;
                }
                case TileType.Tax: TryDeductCash(p, t.taxAmount); break;
                case TileType.Railroad:
                {
                    int ro = tileOwner[pos[p]];
                    if (ro == -1)
                    {
                        if (cash[p] >= t.price)
                            pendingBuyTileIndex = pos[p];
                    }
                    else if (ro != p)
                    {
                        int owned = CountOwnedByType(ro, TileType.Railroad);
                        int rent = config.railroadRentTiers[Mathf.Clamp(owned - 1, 0, config.railroadRentTiers.Length - 1)];
                        if (!TryDeductCash(p, rent)) break;
                        cash[ro] += rent;
                    }
                    break;
                }
                case TileType.Utility:
                {
                    int uo = tileOwner[pos[p]];
                    if (uo == -1)
                    {
                        if (cash[p] >= t.price)
                            pendingBuyTileIndex = pos[p];
                    }
                    else if (uo != p)
                    {
                        int owned = CountOwnedByType(uo, TileType.Utility);
                        int factor = config.utilityRentFactors[Mathf.Clamp(owned - 1, 0, config.utilityRentFactors.Length - 1)];
                        int rent = (lastD1 + lastD2) * factor;
                        if (!TryDeductCash(p, rent)) break;
                        cash[uo] += rent;
                    }
                    break;
                }
                case TileType.Chance:
                {
                    CardDef card = DrawCard(true);
                    lastCardDescription = "CHANCE: " + card.description;
                    cardRevealTimer = 2f;
                    ResolveCard(p, card);
                    break;
                }
                case TileType.CommunityChest:
                {
                    CardDef card = DrawCard(false);
                    lastCardDescription = "CHEST: " + card.description;
                    cardRevealTimer = 2f;
                    ResolveCard(p, card);
                    break;
                }
                case TileType.GoToJail: SendToJail(p); break;
            }
            if (!gameOver) CheckWinLose(p);
        }

        private void EndTurn()
        {
            totalTurns++;
            currentPlayer = (currentPlayer + 1) % playerCount;
        }

        private void SendToJail(int p)
        {
            pos[p] = Mathf.Clamp(config.jailTileIndex, 0, config.tiles.Length - 1);
            jailTurns[p] = 3;
            doublesInRow[p] = 0;
        }

        private int CountOwnedInGroup(int player, ColorGroup group)
        {
            int count = 0;
            for (int i = 0; i < config.tiles.Length; i++)
            {
                if (config.tiles[i].type == TileType.Property &&
                    config.tiles[i].colorGroup == group &&
                    tileOwner[i] == player)
                    count++;
            }
            return count;
        }

        private int CountTilesInGroup(ColorGroup group)
        {
            int count = 0;
            for (int i = 0; i < config.tiles.Length; i++)
            {
                if (config.tiles[i].type == TileType.Property && config.tiles[i].colorGroup == group)
                    count++;
            }
            return count;
        }

        private bool OwnsFullGroup(int player, ColorGroup group)
        {
            return group != ColorGroup.None && CountOwnedInGroup(player, group) == CountTilesInGroup(group);
        }

        private int GetPropertyRent(int tileIndex)
        {
            TileDef t = config.tiles[tileIndex];
            int level = developmentLevels[tileIndex];
            if (t.rentTable != null && t.rentTable.Length > level)
                return t.rentTable[level];
            return t.baseRent;
        }

        private int CountOwnedByType(int player, TileType type)
        {
            int count = 0;
            for (int i = 0; i < config.tiles.Length; i++)
            {
                if (config.tiles[i].type == type && tileOwner[i] == player)
                    count++;
            }
            return count;
        }

        private CardDef DrawCard(bool isChance)
        {
            CardDef[] deck = isChance ? config.chanceCards : config.communityChestCards;
            List<int> order = isChance ? chanceDeckOrder : communityChestDeckOrder;
            ref int drawIndex = ref (isChance ? ref chanceDrawIndex : ref communityChestDrawIndex);

            if (deck == null || deck.Length == 0)
                return new CardDef { type = CardType.GainMoney, amount = 0, description = "Empty deck" };

            if (drawIndex >= order.Count)
            {
                for (int i = order.Count - 1; i > 0; i--)
                {
                    int j = rng.Next(0, i + 1);
                    (order[i], order[j]) = (order[j], order[i]);
                }
                drawIndex = 0;
            }

            CardDef card = deck[order[drawIndex]];
            drawIndex++;
            return card;
        }

        private void ResolveCard(int p, CardDef card)
        {
            switch (card.type)
            {
                case CardType.GainMoney:
                {
                    int gain = card.amount * gainMultiplier;
                    cash[p] += gain;
                    totalMoneyEarned += gain;
                    break;
                }
                case CardType.LoseMoney:
                    TryDeductCash(p, card.amount);
                    break;
                case CardType.GoToTile:
                {
                    int from = pos[p];
                    int to = card.tileIndex;
                    if (to < from)
                    {
                        cash[p] += config.goPayout * gainMultiplier;
                        totalMoneyEarned += config.goPayout * gainMultiplier;
                    }
                    pos[p] = to;
                    // Only resolve landing for non-card tiles to prevent infinite recursion
                    TileType destType = config.tiles[to].type;
                    if (destType != TileType.Chance && destType != TileType.CommunityChest)
                        ResolveLanding(p);
                    break;
                }
                case CardType.GoToJail:
                    SendToJail(p);
                    break;
                case CardType.RepairCosts:
                {
                    int cost = 0;
                    for (int i = 0; i < developmentLevels.Length; i++)
                    {
                        if (tileOwner[i] == p)
                        {
                            int level = developmentLevels[i];
                            if (level >= 5)
                                cost += card.perHotel;
                            else if (level > 0)
                                cost += card.perHouse * level;
                        }
                    }
                    TryDeductCash(p, cost);
                    break;
                }
                case CardType.GainPerProperty:
                {
                    int propCount = 0;
                    for (int i = 0; i < tileOwner.Length; i++)
                    {
                        if (tileOwner[i] == p) propCount++;
                    }
                    int total = card.amount * propCount * gainMultiplier;
                    cash[p] += total;
                    totalMoneyEarned += total;
                    break;
                }
            }
        }

        // Task 10: House/hotel building logic
        private bool CanBuildOnTile(int player, int tileIndex)
        {
            TileDef t = config.tiles[tileIndex];
            if (t.type != TileType.Property) return false;
            if (tileOwner[tileIndex] != player) return false;
            if (!OwnsFullGroup(player, t.colorGroup)) return false;
            int level = developmentLevels[tileIndex];
            if (level >= 5) return false;
            int cost = level < 4 ? t.houseCost : t.hotelCost;
            return cash[player] >= cost;
        }

        private void BuildHouse(int player, int tileIndex)
        {
            if (!CanBuildOnTile(player, tileIndex)) return;
            TileDef t = config.tiles[tileIndex];
            int level = developmentLevels[tileIndex];
            int cost = level < 4 ? t.houseCost : t.hotelCost;
            if (!TryDeductCash(player, cost)) return;
            developmentLevels[tileIndex]++;
        }

        private List<int> GetBuildableTiles(int player)
        {
            List<int> result = new();
            for (int i = 0; i < config.tiles.Length; i++)
            {
                if (CanBuildOnTile(player, i)) result.Add(i);
            }
            return result;
        }

        // Task 11: Decline-to-discount logic
        private int GetPropertyPrice(int tileIndex)
        {
            int basePrice = config.tiles[tileIndex].price;
            if (declinedProperties.Contains(tileIndex))
                return Mathf.RoundToInt(basePrice * 0.8f);
            return basePrice;
        }

        private void BuyProperty(int player, int tileIndex)
        {
            int price = GetPropertyPrice(tileIndex);
            if (cash[player] < price) return;
            if (!TryDeductCash(player, price)) return;
            tileOwner[tileIndex] = player;
            CheckWinLose(player);
            declinedProperties.Remove(tileIndex);
        }

        private void DeclineProperty(int tileIndex)
        {
            if (declinedProperties.Contains(tileIndex))
                declinedProperties.Remove(tileIndex);
            else
                declinedProperties.Add(tileIndex);
        }

        // Task 12: Win/lose condition checks
        private void CheckWinLose(int player)
        {
            if (cash[player] < 0)
            {
                gameOver = true;
                playerWon = false;
                return;
            }

            bool ownsAll = true;
            for (int i = 0; i < config.tiles.Length; i++)
            {
                TileType tt = config.tiles[i].type;
                if (tt == TileType.Property || tt == TileType.Railroad || tt == TileType.Utility)
                {
                    if (tileOwner[i] != player)
                    {
                        ownsAll = false;
                        break;
                    }
                }
            }
            if (ownsAll)
            {
                gameOver = true;
                playerWon = true;
            }
        }

        private bool TryDeductCash(int player, int amount)
        {
            if (cash[player] < amount)
            {
                gameOver = true;
                playerWon = false;
                return false;
            }
            cash[player] -= amount;
            return true;
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
                int devTotal = 0;
                int propsOwned = 0;
                for (int i = 0; i < config.tiles.Length; i++)
                {
                    if (tileOwner[i] == p) { propsOwned++; devTotal += developmentLevels[i]; }
                }
                statsLines[2 + p].text = $"P{p} ${cash[p]} Props:{propsOwned} Dev:{devTotal} T{tile}";
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
