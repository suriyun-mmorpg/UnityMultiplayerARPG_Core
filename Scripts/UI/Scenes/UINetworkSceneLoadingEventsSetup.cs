using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UINetworkSceneLoadingEventsSetup : MonoBehaviour
    {
        public LiteNetLibAssets assets;

        private void Awake()
        {
            assets.onLoadSceneStart.AddListener(UINetworkSceneLoading.Singleton.OnLoadSceneStart);
            assets.onLoadSceneProgress.AddListener(UINetworkSceneLoading.Singleton.OnLoadSceneProgress);
            assets.onLoadSceneFinish.AddListener(UINetworkSceneLoading.Singleton.OnLoadSceneFinish);
        }

        private void OnDestroy()
        {
            assets.onLoadSceneStart.AddListener(UINetworkSceneLoading.Singleton.OnLoadSceneStart);
            assets.onLoadSceneProgress.AddListener(UINetworkSceneLoading.Singleton.OnLoadSceneProgress);
            assets.onLoadSceneFinish.AddListener(UINetworkSceneLoading.Singleton.OnLoadSceneFinish);
        }
    }
}
