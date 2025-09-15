using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MonopolyLite
{
    public sealed class MonoBehaviourPool<T> : BasePool<T> where T : PoolableMonoBehaviour
    {
        private readonly Transform container;

        private readonly T prefab;

        public MonoBehaviourPool(T prefab, Transform container, int capacity = 8, int prewarmCount = 4, int maxCount = 0) : base(capacity, prewarmCount, maxCount)
        {
            this.prefab = prefab;
            this.container = container;
            Prewarm(prewarmCount);
        }

        public int TotalEntryCountClamped => TotalEntryCount;
        public Transform Container => container;

        protected override T Create()
        {
            T inst = Object.Instantiate(prefab, container);
            inst.gameObject.SetActive(false);
            inst.PoolId = Guid.NewGuid();
            inst.IsPooled = true;
            return inst;
        }

        protected override void OnBeforeGet(T item)
        {
            item.IsPooled = false;
            item.gameObject.SetActive(true);
        }

        protected override void OnBeforeRelease(T item)
        {
            item.IsPooled = true;
            item.gameObject.SetActive(false);
            item.transform.SetParent(container, false);
        }

        public T Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            T it = Get();
            if (it == null) return null;
            it.transform.SetParent(parent ? parent : container, false);
            it.transform.SetPositionAndRotation(position, rotation);
            return it;
        }

        public void Despawn(T item)
        {
            Release(item);
        }

        public void DespawnAll()
        {
            T[] inScene = container.GetComponentsInChildren<T>(true);
            for (int i = 0; i < inScene.Length; i++)
                if (!inScene[i].IsPooled)
                    Release(inScene[i]);
        }

        public void Warm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (AvailableCount + InUseCount >= maxCount && maxCount > 0) break;
                T e = Create();
                TotalEntryCount++;
                pool.Push(e);
            }
        }

        public async UniTask ShaderWarmUp(Camera cam = null, Sprite fallbackSprite = null, float displaySeconds = 0.25f)
        {
            await Yield.WaitForUpdate();

#if UNITY_EDITOR
            if (!Mathf.Approximately(1f, Time.timeScale)) return;
#endif
            Camera useCam = cam != null ? cam : Camera.main;
            T clone = Object.Instantiate(prefab);
            Transform tr = clone.transform;
            Vector3 pos = Vector3.zero;
            if (useCam != null)
            {
                pos.x = useCam.transform.position.x;
                pos.y = useCam.transform.position.y;
            }

            tr.position = pos;

            Behaviour despawnWithDelay = clone.GetComponent<Behaviour>();
            if (despawnWithDelay != null) despawnWithDelay.enabled = false;

            foreach (Transform child in tr.GetAllChildren())
            {
                child.gameObject.SetActive(true);
                child.localScale = Vector3.one;
                if (child.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr) && sr.sprite == null && fallbackSprite != null) sr.sprite = fallbackSprite;
            }

            await Yield.WaitForUpdate();
            tr.position += Vector3.up;
            await Yield.WaitForSeconds(displaySeconds);
            Object.Destroy(clone.gameObject);
        }
    }
}