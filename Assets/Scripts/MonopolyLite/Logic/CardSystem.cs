using MonopolyLite;
using MonopolyLite.Data;
using MonopolyLite.State;

namespace MonopolyLite.Logic
{
    /// Design decision: Card GainCoins scales with multiplier (rewarding high-risk play).
    /// Card LoseCoins does NOT scale (flat penalty). This matches Monopoly Go behavior
    /// where positive outcomes are amplified but negative card outcomes are fixed.
    public class CardSystem
    {
        readonly int _seed;
        readonly MovementSystem _movementSystem;
        int[] _chanceOrder;
        int[] _communityChestOrder;

        public CardSystem(int seed, MovementSystem movementSystem = null)
        {
            _seed = seed;
            _movementSystem = movementSystem;
        }

        public CardDef DrawChance(GameState state)
        {
            var cards = state.BoardDef.chanceCards;
            if (_chanceOrder == null || _chanceOrder.Length != cards.Length)
                _chanceOrder = CreateShuffledIndices(cards.Length, _seed);

            int idx = state.Board.ChanceDrawIndex;
            var card = cards[_chanceOrder[idx]];
            state.Board.ChanceDrawIndex = (idx + 1) % cards.Length;
            return card;
        }

        public CardDef DrawCommunityChest(GameState state)
        {
            var cards = state.BoardDef.communityChestCards;
            if (_communityChestOrder == null || _communityChestOrder.Length != cards.Length)
                _communityChestOrder = CreateShuffledIndices(cards.Length, _seed + 1);

            int idx = state.Board.CommunityChestDrawIndex;
            var card = cards[_communityChestOrder[idx]];
            state.Board.CommunityChestDrawIndex = (idx + 1) % cards.Length;
            return card;
        }

        public void ApplyCard(GameState state, CardDef card)
        {
            switch (card.type)
            {
                case CardType.GainCoins:
                    state.Player.AddCoins(card.amount * state.Player.Multiplier);
                    break;
                case CardType.LoseCoins:
                    state.Player.SpendCoins(card.amount); // flat, no multiplier
                    break;
                case CardType.GainDice:
                    state.Player.AddDice(card.amount);
                    break;
                case CardType.GainShield:
                    state.Player.AddShield(card.amount);
                    break;
                case CardType.GoToJail:
                    state.Player.Position = state.BoardDef.jailTileIndex;
                    state.Player.JailTurnsLeft = 3;
                    break;
                case CardType.GoToTile:
                    if (_movementSystem != null)
                        _movementSystem.MoveToTile(state, card.tileIndex, grantGoBonus: true);
                    else
                        state.Player.Position = card.tileIndex;
                    break;
            }
        }

        static int[] CreateShuffledIndices(int count, int seed)
        {
            var indices = new int[count];
            for (int i = 0; i < count; i++) indices[i] = i;
            // Fisher-Yates shuffle using existing RNG from Helpers.cs
            var rng = new RNG((uint)seed);
            for (int i = count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }
            return indices;
        }
    }
}
