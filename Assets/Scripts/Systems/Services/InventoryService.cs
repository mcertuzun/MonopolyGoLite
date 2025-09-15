using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace MonopolyLite
{
    public sealed class InventoryService : GameService
    {
        private readonly Dictionary<string, int> bag = new();
        protected override string LOG_NAME => "inventory";

        public override async UniTask Start()
        {
            await UniTask.CompletedTask;
        }

        public override async UniTask<bool> Restart()
        {
            await UniTask.CompletedTask;
            return true;
        }

        public int Get(string id)
        {
            return bag.TryGetValue(id, out int v) ? v : 0;
        }

        public void Add(string id, int v)
        {
            bag[id] = Get(id) + v;
            Logger.Info($"{id}={bag[id]}");
        }

        public bool Spend(string id, int v)
        {
            int cur = Get(id);
            if (cur < v) return false;
            bag[id] = cur - v;
            Logger.Info($"{id}={bag[id]}");
            return true;
        }
    }
}