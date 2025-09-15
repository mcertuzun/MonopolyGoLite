using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MonopolyLite
{
    public sealed class ProfileService : GameService
    {
        protected override string LOG_NAME => "profile";
        public string PlayerId { get; private set; }
        public string DisplayName { get; private set; }

        public override async UniTask Start()
        {
            PlayerId = "player_" + Random.Range(1000, 9999);
            DisplayName = "Player";
            await UniTask.CompletedTask;
        }

        public override async UniTask<bool> Restart()
        {
            await Start();
            return true;
        }
    }
}