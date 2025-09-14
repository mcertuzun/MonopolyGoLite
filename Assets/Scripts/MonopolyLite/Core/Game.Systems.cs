using UnityEngine;

namespace MonopolyLite
{
    public partial class MonopolyLiteGame
    {
        private void ApplyCommand(ref State s, Command cmd)
        {
            switch (cmd.type)
            {
                case CommandType.RollDice: ExecuteRollDice(ref s, cmd.player); break;
                case CommandType.EndTurn: EndTurn(ref s); break;
                case CommandType.PurchaseProperty: TryPurchase(ref s, cmd.player); break;
                case CommandType.SetMultiplier: s.gainMultiplier = Mathf.Clamp(cmd.value, 1, 3); break;
            }
        }

        private void RunSystems(ref State s)
        {
            s.diceChargeTimer += 1f / Deterministic.FixedHz;
            if (s.diceChargeTimer >= s.diceChargeInterval)
            {
                s.diceChargeTimer -= s.diceChargeInterval;
                if (s.diceCharges < s.diceChargeCap) s.diceCharges++;
            }
        }

        private void ExecuteRollDice(ref State s, int player)
        {
            if (player != s.currentPlayer) return;
            if (s.diceCharges <= 0) return;
            s.diceCharges--;

            if (s.jailTurns[player] > 0)
            {
                s.jailTurns[player]--;
                EndTurn(ref s);
                return;
            }

            int d1 = s.rng.Next(1, 7);
            int d2 = s.rng.Next(1, 7);
            s.lastD1 = d1;
            s.lastD2 = d2;
            bool isDouble = d1 == d2;
            int steps = d1 + d2;
            int prev = s.pos[player];
            int next = (prev + steps) % s.boardConfig.tiles.Length;
            bool passedGo = prev + steps >= s.boardConfig.tiles.Length;
            s.pos[player] = next;
            if (passedGo)
            {
                int gain = s.goPayout * s.liveOps.goPayoutMultiplier;
                s.cash[player] += gain * s.gainMultiplier;
            }

            ResolveLanding(ref s, player);
            if (isDouble)
            {
                s.doublesInRow[player]++;
                if (s.doublesInRow[player] >= 3)
                {
                    SendToJail(ref s, player);
                    EndTurn(ref s);
                }
            }
            else
            {
                s.doublesInRow[player] = 0;
                EndTurn(ref s);
            }
        }

        private void EndTurn(ref State s)
        {
            s.currentPlayer = (s.currentPlayer + 1) % s.playerCount;
        }

        private void SendToJail(ref State s, int player)
        {
            s.pos[player] = s.boardConfig.jailTileIndex;
            s.jailTurns[player] = 3;
            s.doublesInRow[player] = 0;
        }

        private void ResolveLanding(ref State s, int player)
        {
            Tile tile = s.boardConfig.tiles[s.pos[player]];
            switch (tile.type)
            {
                case TileType.Go:
                    break;
                case TileType.Property:
                    int owner = s.tileOwner[s.pos[player]];
                    if (owner == -1)
                    {
                        if (s.cash[player] >= tile.price)
                        {
                            s.cash[player] -= tile.price;
                            s.tileOwner[s.pos[player]] = player;
                        }
                    }
                    else if (owner != player)
                    {
                        int rent = Mathf.RoundToInt(tile.baseRent * s.liveOps.rentMultiplier);
                        s.cash[player] -= rent;
                        s.cash[owner] += rent;
                    }

                    break;
                case TileType.Tax:
                    s.cash[player] -= tile.taxAmount;
                    break;
                case TileType.Chest:
                    int delta = s.rng.Next(-50, 101);
                    if (delta > 0) delta *= s.gainMultiplier;
                    s.cash[player] += delta;
                    break;
                case TileType.GoToJail:
                    SendToJail(ref s, player);
                    break;
                case TileType.Jail:
                    break;
            }
        }

        private void TryPurchase(ref State s, int player)
        {
            Tile tile = s.boardConfig.tiles[s.pos[player]];
            if (tile.type != TileType.Property) return;
            if (s.tileOwner[s.pos[player]] != -1) return;
            if (s.cash[player] < tile.price) return;
            s.cash[player] -= tile.price;
            s.tileOwner[s.pos[player]] = player;
        }
    }
}