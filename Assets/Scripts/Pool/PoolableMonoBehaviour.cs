using System;
using UnityEngine;

namespace MonopolyLite
{
    public abstract class PoolableMonoBehaviour : MonoBehaviour, IPoolItem
    {
        public Guid PoolId { get; internal set; }
        public bool IsPooled { get; internal set; }
        public virtual void OnSpawned() { }
        public virtual void OnDespawned() { }
    }
}