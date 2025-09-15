using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonopolyLite
{
    public interface IPoolItem
    {
        void OnSpawned();
        void OnDespawned();
    }

    public abstract class BasePool<T>
    {
        protected readonly int maxCount;
        protected readonly Stack<T> pool;

        protected BasePool(int capacity, int prewarmCount, int maxCount)
        {
            if (capacity < 1) capacity = 1;
            if (prewarmCount < 0) prewarmCount = 0;
            if (prewarmCount > capacity) capacity = prewarmCount;
            this.maxCount = maxCount <= 0 ? int.MaxValue : maxCount;
            pool = new Stack<T>(capacity);
        }

        public int AvailableCount => pool.Count;
        public int InUseCount => Math.Max(0, TotalEntryCount - AvailableCount);
        public int TotalEntryCount { get; protected set; }
        public int SpawnCount { get; protected set; }

        protected abstract T Create();
        protected virtual void OnBeforeGet(T item) { }
        protected virtual void OnBeforeRelease(T item) { }

        public virtual void Prewarm(int count)
        {
            int target = Mathf.Clamp(count, 0, maxCount);
            for (int i = 0; i < target && TotalEntryCount < maxCount; i++)
            {
                T it = Create();
                TotalEntryCount++;
                pool.Push(it);
            }
        }

        public virtual T Get()
        {
            T it = pool.Count > 0 ? pool.Pop() : CreateOrNull();
            if (it == null) return default;
            SpawnCount++;
            OnBeforeGet(it);
            if (it is IPoolItem p) p.OnSpawned();
            return it;
        }

        public virtual void Release(T item)
        {
            if (item is IPoolItem p) p.OnDespawned();
            OnBeforeRelease(item);
            if (TotalEntryCount <= maxCount) pool.Push(item);
        }

        private T CreateOrNull()
        {
            if (TotalEntryCount >= maxCount) return default;
            T it = Create();
            TotalEntryCount++;
            return it;
        }
    }

    public sealed class ObjectPool<T> : BasePool<T> where T : class, new()
    {
        private readonly Action<T> onGet;
        private readonly Action<T> onRelease;

        public ObjectPool(int capacity = 8, int prewarmCount = 4, int maxCount = 0, Action<T> onGet = null, Action<T> onRelease = null)
            : base(capacity, prewarmCount, maxCount)
        {
            this.onGet = onGet;
            this.onRelease = onRelease;
            Prewarm(prewarmCount);
        }

        protected override T Create()
        {
            return new T();
        }

        protected override void OnBeforeGet(T item)
        {
            onGet?.Invoke(item);
        }

        protected override void OnBeforeRelease(T item)
        {
            onRelease?.Invoke(item);
        }
    }
}