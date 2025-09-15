using Cysharp.Threading.Tasks;

namespace MonopolyLite
{
    public sealed class NetworkService : GameService
    {
        protected override string LOG_NAME => "net";
        public bool IsOnline { get; private set; }

        public override async UniTask Start()
        {
            await Ping();
        }

        public override async UniTask<bool> Restart()
        {
            await Ping();
            return IsOnline;
        }

        public async UniTask Ping()
        {
            await WaitForSeconds(0.1f);
            IsOnline = true;
            Logger.Info("online");
        }
    }
}