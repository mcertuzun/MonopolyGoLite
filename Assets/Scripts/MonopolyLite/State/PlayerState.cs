namespace MonopolyLite.State
{
    public class PlayerState
    {
        public int Coins { get; private set; }
        public int Dice { get; private set; }
        public int DiceCap { get; private set; }
        public int Position { get; set; }
        public int Shields { get; set; }
        public int NetWorth { get; set; }
        public int Multiplier { get; set; } = 1;
        public int JailTurnsLeft { get; set; }

        public PlayerState(int startingDice, int diceCap)
        {
            Dice = startingDice;
            DiceCap = diceCap;
        }

        public void AddCoins(int amount) { Coins += amount; }

        public bool SpendCoins(int amount)
        {
            if (Coins < amount) return false;
            Coins -= amount;
            return true;
        }

        public int DeductCoins(int amount)
        {
            int actual = System.Math.Min(amount, Coins);
            Coins -= actual;
            return actual;
        }

        public bool ConsumeDice()
        {
            int cost = Multiplier;
            if (Dice < cost) return false;
            Dice -= cost;
            return true;
        }

        public void AddDice(int amount)
        {
            Dice = System.Math.Min(Dice + amount, DiceCap);
        }

        public bool SpendDice(int amount)
        {
            if (Dice < amount) return false;
            Dice -= amount;
            return true;
        }

        public void AddShield(int count = 1)
        {
            Shields = System.Math.Min(Shields + count, 3);
        }
    }
}
