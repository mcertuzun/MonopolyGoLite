using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MonopolyLite
{
    public static class Yield
    {
        private static int nextIndex = 1;
        private static readonly Dictionary<int, CancellationTokenSource> map = new();

        public static int AddInternalContext()
        {
            int context = nextIndex++;
            map[context] = new CancellationTokenSource();
            return context;
        }

        public static int AddInternalLinkedContext(int parent)
        {
            if (!map.TryGetValue(parent, out CancellationTokenSource p)) return AddInternalContext();
            CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(p.Token);
            int context = nextIndex++;
            map[context] = linked;
            return context;
        }

        public static CancellationToken GetCancellationToken(int index)
        {
            return map.TryGetValue(index, out CancellationTokenSource cts) ? cts.Token : CancellationToken.None;
        }

        public static void CancelInternalContext(int index)
        {
            if (map.TryGetValue(index, out CancellationTokenSource cts))
            {
                cts.Cancel();
                cts.Dispose();
            }

            map.Remove(index);
        }

        public static async UniTask WaitForSeconds(float sec, int context = 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0f, sec)), cancellationToken: GetCancellationToken(context));
        }

        public static async UniTask WaitForFixedSeconds(float sec, int context)
        {
            float end = Time.realtimeSinceStartup + Mathf.Max(0f, sec);
            CancellationToken token = GetCancellationToken(context);
            while (Time.realtimeSinceStartup < end && !token.IsCancellationRequested) await UniTask.WaitForFixedUpdate(token);
        }

        public static async UniTask<float> WaitForRandomSeconds(float min, float max, int context)
        {
            float v = Random.Range(min, max);
            await WaitForSeconds(v, context);
            return v;
        }

        public static async UniTask WaitForUpdate(int context = 0)
        {
            await UniTask.Yield(GetCancellationToken(context));
        }

        public static async UniTask WaitForFixedUpdate(int context = 0)
        {
            await UniTask.WaitForFixedUpdate(GetCancellationToken(context));
        }

        public static async UniTask WaitForEndOfFrame(int context = 0)
        {
            await UniTask.WaitForEndOfFrame(GetCancellationToken(context));
        }

        public static async UniTask WaitForEditorUpdate(int context = 0)
        {
            await UniTask.Yield(GetCancellationToken(context));
        }

        public static async UniTask WaitForUpdateCount(int c, int context)
        {
            CancellationToken token = GetCancellationToken(context);
            for (int i = 0; i < Mathf.Max(1, c); i++) await UniTask.Yield(token);
        }

        public static async UniTask WaitUntil(Func<bool> p, int context)
        {
            await UniTask.WaitUntil(p, cancellationToken: GetCancellationToken(context));
        }

        public static async UniTask WaitAndInvoke(float sec, Action cb, int context)
        {
            await WaitForSeconds(sec, context);
            if (!GetCancellationToken(context).IsCancellationRequested) cb?.Invoke();
        }

        public static async UniTask WaitForNextFrame(int context = 0)
        {
            await UniTask.NextFrame(GetCancellationToken(context));
        }
    }
}