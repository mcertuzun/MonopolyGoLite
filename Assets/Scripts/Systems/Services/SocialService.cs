using Cysharp.Threading.Tasks;

namespace MonopolyLite
{
    public sealed class SocialService : GameService
    {
        protected override string LOG_NAME => "social";

        public override async UniTask Start()
        {
            await UniTask.CompletedTask;
        }

        public override async UniTask<bool> Restart()
        {
            await UniTask.CompletedTask;
            return true;
        }

        public void ShareText(string text)
        {
            Logger.Info($"share: {text}");
        }
    }
}