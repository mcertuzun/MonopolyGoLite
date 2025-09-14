using System;

namespace MonopolyLite
{
    public enum CommandType
    {
        RollDice,
        EndTurn,
        PurchaseProperty,
        SetMultiplier
    }

    [Serializable]
    public struct Command
    {
        public CommandType type;
        public int player;
        public int value;

        public static Command Roll(int player)
        {
            return new Command
            { type = CommandType.RollDice, player = player };
        }

        public static Command End()
        {
            return new Command
            { type = CommandType.EndTurn };
        }

        public static Command Purchase(int player)
        {
            return new Command
            { type = CommandType.PurchaseProperty, player = player };
        }

        public static Command SetMult(int v)
        {
            return new Command
            { type = CommandType.SetMultiplier, value = v };
        }
    }
}