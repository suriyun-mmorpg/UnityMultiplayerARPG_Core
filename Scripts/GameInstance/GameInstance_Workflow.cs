using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    public partial class GameInstance
    {
        [Header("Home Scene")]
        public SceneField homeScene;
        public AssetReferenceScene addressableHomeScene;
        [Tooltip("If this is empty, it will use `Home Scene` as home scene")]
        public SceneField homeMobileScene;
        public AssetReferenceScene addressableHomeMobileScene;
        [Tooltip("If this is empty, it will use `Home Scene` as home scene")]
        public SceneField homeConsoleScene;
        public AssetReferenceScene addressableHomeConsoleScene;

        public void LoadHomeScene()
        {
            StartCoroutine(LoadHomeSceneRoutine());
        }

        IEnumerator LoadHomeSceneRoutine()
        {
            if (UISceneLoading.Singleton)
            {
                if (GetHomeScene(out SceneField scene, out AssetReferenceScene addressableScene))
                {
                    yield return UISceneLoading.Singleton.LoadScene(addressableScene);
                }
                else
                {
                    yield return UISceneLoading.Singleton.LoadScene(scene);
                }
            }
            else
            {
                if (GetHomeScene(out SceneField scene, out AssetReferenceScene addressableScene))
                {
                    yield return addressableScene.LoadSceneAsync();
                }
                else
                {
                    yield return SceneManager.LoadSceneAsync(scene);
                }
            }
        }

        /// <summary>
        /// Return `TRUE` if it is addressable
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="addressableScene"></param>
        /// <returns></returns>
        public bool GetHomeScene(out SceneField scene, out AssetReferenceScene addressableScene)
        {
            addressableScene = null;
            scene = default;
            if (Application.isMobilePlatform || IsMobileTestInEditor())
            {
                if (addressableHomeMobileScene.IsDataValid())
                {
                    addressableScene = addressableHomeMobileScene;
                    return true;
                }
                scene = homeMobileScene;
                return false;
            }
            if (Application.isConsolePlatform || IsConsoleTestInEditor())
            {
                if (addressableHomeConsoleScene.IsDataValid())
                {
                    addressableScene = addressableHomeConsoleScene;
                    return true;
                }
                scene = homeConsoleScene;
                return false;
            }
            if (addressableHomeScene.IsDataValid())
            {
                addressableScene = addressableHomeScene;
                return true;
            }
            scene = homeScene;
            return false;
        }
    }
}
