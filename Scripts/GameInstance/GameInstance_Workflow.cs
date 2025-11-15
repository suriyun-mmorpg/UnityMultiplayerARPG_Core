using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
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

        private static List<AsyncOperationHandle<SceneInstance>> s_loadingAddressableSceneHandles = new List<AsyncOperationHandle<SceneInstance>>();

        public static AsyncOperationHandle<SceneInstance> LoadAddressableScene(AssetReferenceScene addressableScene, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            AsyncOperationHandle<SceneInstance> addressableAsyncOp = addressableScene.LoadSceneAsync(loadSceneMode, true);
            s_loadingAddressableSceneHandles.Add(addressableAsyncOp);
            return addressableAsyncOp;
        }

        public static async UniTask UnloadAddressableScenes()
        {
            if (s_loadingAddressableSceneHandles.Count == 0)
                return;
            for (int i = 0; i < s_loadingAddressableSceneHandles.Count; ++i)
            {
                AsyncOperationHandle<SceneInstance> addressableAsyncOp = Addressables.UnloadSceneAsync(s_loadingAddressableSceneHandles[i], UnloadSceneOptions.UnloadAllEmbeddedSceneObjects, true);
                while (!addressableAsyncOp.IsDone)
                {
                    await UniTask.Yield();
                }
            }
            s_loadingAddressableSceneHandles.Clear();
        }

        public async void LoadHomeScene()
        {
            await LoadHomeSceneTask();
        }

        public async UniTask LoadHomeSceneTask()
        {
            await UnloadAddressableScenes();
            if (UISceneLoading.Singleton)
            {
                if (GetHomeScene(out SceneField scene, out AssetReferenceScene addressableScene))
                {
                    await UISceneLoading.Singleton.LoadScene(addressableScene);
                }
                else
                {
                    await UISceneLoading.Singleton.LoadScene(scene);
                }
            }
            else
            {
                if (GetHomeScene(out SceneField scene, out AssetReferenceScene addressableScene))
                {
                    await LoadAddressableScene(addressableScene);
                }
                else
                {
                    await SceneManager.LoadSceneAsync(scene).ToUniTask();
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
