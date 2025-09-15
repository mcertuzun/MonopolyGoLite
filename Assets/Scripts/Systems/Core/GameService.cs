using Cysharp.Threading.Tasks;

namespace MonopolyLite
{
    public abstract class GameService : GameSystem
    {
        internal override Logger.LoggerFormatting LOG_FORMATTING => new("lime", base.LOG_FORMATTING.MessageColor, base.LOG_FORMATTING.CallerColor);
        public abstract UniTask<bool> Restart();
    }
}