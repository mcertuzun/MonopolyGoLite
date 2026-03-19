using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    public enum TileResolveType
    {
        Nothing, CoinsGained, CoinsLost, Card, Jail, Railroad
    }

    public struct TileResolveResult
    {
        public TileResolveType Type;
        public int Amount;
        public CardDef? DrawnCard;
    }

    public class TileResolver
    {
        readonly CardSystem _cardSystem;
        readonly JailSystem _jailSystem;

        public TileResolver(CardSystem cardSystem, JailSystem jailSystem)
        {
            _cardSystem = cardSystem;
            _jailSystem = jailSystem;
        }

        public TileResolveResult Resolve(GameState state)
        {
            var tile = state.BoardDef.tiles[state.Player.Position];

            switch (tile.type)
            {
                case TileType.Property:
                {
                    int reward = tile.baseReward * state.Player.Multiplier;
                    state.Player.AddCoins(reward);
                    return new TileResolveResult { Type = TileResolveType.CoinsGained, Amount = reward };
                }
                case TileType.Tax:
                {
                    int loss = tile.taxAmount * state.Player.Multiplier;
                    state.Player.SpendCoins(loss);
                    return new TileResolveResult { Type = TileResolveType.CoinsLost, Amount = loss };
                }
                case TileType.Railroad:
                {
                    // Phase 3: Bank Heist / Shutdown. Placeholder: bonus coins.
                    int reward = tile.baseReward * state.Player.Multiplier;
                    state.Player.AddCoins(reward);
                    return new TileResolveResult { Type = TileResolveType.Railroad, Amount = reward };
                }
                case TileType.Chance:
                {
                    var card = _cardSystem.DrawChance(state);
                    _cardSystem.ApplyCard(state, card);
                    return new TileResolveResult { Type = TileResolveType.Card, DrawnCard = card };
                }
                case TileType.CommunityChest:
                {
                    var card = _cardSystem.DrawCommunityChest(state);
                    _cardSystem.ApplyCard(state, card);
                    return new TileResolveResult { Type = TileResolveType.Card, DrawnCard = card };
                }
                case TileType.GoToJail:
                {
                    _jailSystem.SendToJail(state);
                    return new TileResolveResult { Type = TileResolveType.Jail };
                }
                case TileType.Go:
                case TileType.Jail:
                case TileType.FreeParking:
                default:
                    return new TileResolveResult { Type = TileResolveType.Nothing };
            }
        }
    }
}
