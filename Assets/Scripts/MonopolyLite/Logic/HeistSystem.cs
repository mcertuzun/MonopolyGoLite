using MonopolyLite.Data;

namespace MonopolyLite.Logic
{
    public class HeistSystem
    {
        RNG _rng;

        const int CoinBagReward = 100;
        const int GoldBarReward = 300;
        const int DiamondReward = 1000;
        const int MissReward = 50;

        public HeistSystem(int seed)
        {
            _rng = new RNG((uint)seed);
        }

        public HeistResult Resolve(int multiplier, float boardMultiplier)
        {
            int roll = _rng.Next(0, 100);

            bool isMatch;
            HeistSymbol symbol;
            int baseReward;

            if (roll < 40)      { isMatch = true;  symbol = HeistSymbol.CoinBag; baseReward = CoinBagReward; }
            else if (roll < 70) { isMatch = true;  symbol = HeistSymbol.GoldBar; baseReward = GoldBarReward; }
            else if (roll < 80) { isMatch = true;  symbol = HeistSymbol.Diamond; baseReward = DiamondReward; }
            else                { isMatch = false; symbol = HeistSymbol.CoinBag; baseReward = MissReward; }

            int coinsEarned = (int)(baseReward * multiplier * boardMultiplier);
            var grid = GenerateGrid(isMatch, symbol);

            return new HeistResult
            {
                IsMatch = isMatch,
                MatchedSymbol = symbol,
                CoinsEarned = coinsEarned,
                Grid = grid,
            };
        }

        HeistSymbol[] GenerateGrid(bool isMatch, HeistSymbol matched)
        {
            var grid = new HeistSymbol[12];
            var allSymbols = new[] { HeistSymbol.CoinBag, HeistSymbol.GoldBar, HeistSymbol.Diamond };

            if (isMatch)
            {
                var positions = new bool[12];
                int placed = 0;
                while (placed < 3)
                {
                    int pos = _rng.Next(0, 12);
                    if (!positions[pos])
                    {
                        positions[pos] = true;
                        grid[pos] = matched;
                        placed++;
                    }
                }

                for (int i = 0; i < 12; i++)
                {
                    if (!positions[i])
                        grid[i] = allSymbols[_rng.Next(0, allSymbols.Length)];
                }
            }
            else
            {
                for (int i = 0; i < 12; i++)
                    grid[i] = allSymbols[_rng.Next(0, allSymbols.Length)];
            }

            return grid;
        }
    }
}
