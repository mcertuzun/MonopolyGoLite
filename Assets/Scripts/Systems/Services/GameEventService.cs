using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace MonopolyLite
{
    public sealed class GameEventService : GameService
    {
        private readonly Dictionary<string, Action<object>> map = new();
        protected override string LOG_NAME => "events";

        public override async UniTask Start()
        {
            await UniTask.CompletedTask;
        }

        public override async UniTask<bool> Restart()
        {
            await UniTask.CompletedTask;
            return true;
        }

        public void Subscribe(string key, Action<object> cb)
        {
            if (map.TryGetValue(key, out Action<object> cur)) map[key] = cur + cb;
            else map[key] = cb;
        }

        public void Unsubscribe(string key, Action<object> cb)
        {
            if (!map.TryGetValue(key, out Action<object> cur)) return;
            cur -= cb;
            if (cur == null) map.Remove(key);
            else map[key] = cur;
        }

        public void Publish(string key, object payload = null)
        {
            if (map.TryGetValue(key, out Action<object> cb)) cb?.Invoke(payload);
        }
    }
}