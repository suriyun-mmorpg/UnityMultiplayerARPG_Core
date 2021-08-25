using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class UIAssetBundleErrorHandler : MonoBehaviour
    {
        public UnityEvent onManifestLoadedFail = new UnityEvent();
        public UnityEvent onDependenciesLoadedFail = new UnityEvent();
        public UnityEvent onMainAssetBundleLoadedFail = new UnityEvent();

        private void Start()
        {
            AssetBundleManager.Singleton.onManifestLoadedFail.RemoveListener(OnManifestLoadedFail);
            AssetBundleManager.Singleton.onManifestLoadedFail.AddListener(OnManifestLoadedFail);
            AssetBundleManager.Singleton.onManifestLoadedFail.RemoveListener(OnDependenciesLoadedFail);
            AssetBundleManager.Singleton.onManifestLoadedFail.AddListener(OnDependenciesLoadedFail);
            AssetBundleManager.Singleton.onManifestLoadedFail.RemoveListener(OnMainAssetBundleLoadedFail);
            AssetBundleManager.Singleton.onManifestLoadedFail.AddListener(OnMainAssetBundleLoadedFail);
        }

        private void OnDestroy()
        {
            AssetBundleManager.Singleton.onManifestLoadedFail.RemoveListener(OnManifestLoadedFail);
            AssetBundleManager.Singleton.onDependenciesLoadedFail.RemoveListener(OnDependenciesLoadedFail);
            AssetBundleManager.Singleton.onMainAssetBundleLoadedFail.RemoveListener(OnMainAssetBundleLoadedFail);
        }

        private void OnManifestLoadedFail()
        {

        }

        private void OnDependenciesLoadedFail()
        {

        }

        private void OnMainAssetBundleLoadedFail()
        {

        }
    }
}
