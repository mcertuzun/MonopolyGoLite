using System;
using System.Threading;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MonopolyLite
{
    public abstract class GameSystem
    {
        private int contextIndex;
        public Logger Logger;

        protected GameSystem()
        {
            Logger = new Logger(LOG_NAME, LOG_FORMATTING);
            contextIndex = Yield.AddInternalContext();
        }

        protected abstract string LOG_NAME { get; }
        internal virtual Logger.LoggerFormatting LOG_FORMATTING => new("white", "grey", "grey");
        public int ContextIndex => contextIndex;

        protected int GetSubContext()
        {
            return Yield.AddInternalLinkedContext(ContextIndex);
        }

        protected CancellationToken GetCancellationToken()
        {
            return Yield.GetCancellationToken(ContextIndex);
        }

        protected void CancelInternalContext()
        {
            Yield.CancelInternalContext(ContextIndex);
        }

        public async UniTask WaitForSeconds(float s)
        {
            await Yield.WaitForSeconds(s, ContextIndex);
        }

        public async UniTask<float> WaitForRandomSeconds()
        {
            return await WaitForRandomSeconds(0.3f, 0.5f);
        }

        public async UniTask<float> WaitForRandomSeconds(float a, float b)
        {
            return await Yield.WaitForRandomSeconds(a, b, ContextIndex);
        }

        public async UniTask WaitForUpdate()
        {
            await Yield.WaitForUpdate(ContextIndex);
        }

        public async UniTask WaitForFixedUpdate()
        {
            await Yield.WaitForFixedUpdate(ContextIndex);
        }

        public async UniTask WaitForEditorUpdate()
        {
            await Yield.WaitForEditorUpdate(ContextIndex);
        }

        public async UniTask WaitForUpdateCount(int c = 1)
        {
            await Yield.WaitForUpdateCount(c, ContextIndex);
        }

        public async UniTask WaitForEndOfFrame()
        {
            await Yield.WaitForEndOfFrame(ContextIndex);
        }

        public async UniTask WaitForFixedSeconds(float s)
        {
            await Yield.WaitForFixedSeconds(s, ContextIndex);
        }

        public async UniTask WaitUntil(Func<bool> p)
        {
            await Yield.WaitUntil(p, ContextIndex);
        }

        public async UniTask WaitAndInvoke(float s, Action cb)
        {
            await Yield.WaitAndInvoke(s, cb, ContextIndex);
        }

        public async UniTask WaitForNextFrame()
        {
            await Yield.WaitForNextFrame(ContextIndex);
        }

        public abstract UniTask Start();

        public virtual void Shutdown()
        {
            CancelInternalContext();
        }

        public void Log(string msg = "")
        {
            if (string.IsNullOrEmpty(msg)) Logger.Info("ok");
            else Logger.Info(msg);
        }
    }

    public abstract class SingletonGameSystem<T> : GameSystem where T : GameSystem, new()
    {
        public static T Instance { get; private set; }

        public static T CreateInstance()
        {
            Instance = new T();
            Instance.Log();
            return Instance;
        }

        public static async UniTask StartSingleton()
        {
            if (Instance == null) CreateInstance();
            await Instance.Start();
        }

        public static void ShutdownSingleton()
        {
            if (Instance != null)
            {
                Instance.Log("shutdown");
                Instance.Shutdown();
            }

            Instance = null;
        }
#if UNITY_EDITOR
        static SingletonGameSystem()
        {
            EditorApplication.playModeStateChanged -= OnPM;
            EditorApplication.playModeStateChanged += OnPM;
        }

        private static void OnPM(PlayModeStateChange s)
        {
            if (s == PlayModeStateChange.EnteredEditMode)
                try
                {
                    ShutdownSingleton();
                }
                catch { }
        }
#endif
    }
}