using UnityEngine;

namespace MonopolyLite
{
    public class Bootstrap : MonoBehaviour
    {
        private async void Awake()
        {
            Main app = new GameObject("GameApp").AddComponent<Main>();
            Services.CreateInstance();
            await Services.StartSingleton();
        }

        private void OnDestroy()
        {
            Services.ShutdownSingleton();
        }
    }
}