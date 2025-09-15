using Cysharp.Threading.Tasks;

namespace MonopolyLite
{
    public sealed class MonetizationService : GameService
    {
        protected override string LOG_NAME => "monetization";
        public int SoftCurrency { get; private set; }
        public int HardCurrency { get; private set; }

        public override async UniTask Start()
        {
            await UniTask.CompletedTask;
        }

        public override async UniTask<bool> Restart()
        {
            await UniTask.CompletedTask;
            return true;
        }

        public void AddSoft(int v)
        {
            SoftCurrency += v;
            Logger.Info("soft=" + SoftCurrency);
        }

        public void AddHard(int v)
        {
            HardCurrency += v;
            Logger.Info("hard=" + HardCurrency);
        }
    }
}