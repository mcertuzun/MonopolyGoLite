using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace MonopolyLite
{
    public partial class Services : SingletonGameSystem<Services>
    {
        private readonly Dictionary<Type, GameService> modules = new();
        protected override string LOG_NAME => "modules";

        public GameEventService GameEventService => GetModule<GameEventService>();
        public NetworkService NetworkService => GetModule<NetworkService>();
        public SocialService SocialService => GetModule<SocialService>();
        public ProfileService ProfileService => GetModule<ProfileService>();
        public MonetizationService MonetizationService => GetModule<MonetizationService>();
        public InventoryService InventoryService => GetModule<InventoryService>();

        public T GetModule<T>() where T : GameService
        {
            return (T)modules[typeof(T)];
        }

        public override async UniTask Start()
        {
            Register(new GameEventService());
            Register(new NetworkService());
            Register(new SocialService());
            Register(new ProfileService());
            Register(new MonetizationService());
            Register(new InventoryService());
            foreach (GameService m in modules.Values) await m.Start();
            Log("all started");
        }

        public override void Shutdown()
        {
            foreach (GameService m in modules.Values) m.Shutdown();
            modules.Clear();
            base.Shutdown();
        }

        public void Register(GameService m)
        {
            Type t = m.GetType();
            if (modules.ContainsKey(t)) return;
            modules[t] = m;
            m.Log($"registered {t.Name}");
        }

        public async UniTask<bool> RestartAll()
        {
            bool ok = true;
            foreach (GameService m in modules.Values) ok &= await m.Restart();
            return ok;
        }
    }
}