using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    public partial class AssetBundleManager : MonoBehaviour
    {
        public enum LoadMode
        {
            None,
            FromServerUrl,
            FromLocalPath,
        }

        public enum LoadState
        {
            None,
            LoadManifest,
            LoadDependencies,
            LoadMainAssetBundle,
            Done,
        }

        [Serializable]
        public struct AssetBundleSetting
        {
            public string overrideServerUrl;
            public string overrideLocalFolderPath;
            public LoadMode overrideLoadMode;
            public string platformFolderName;
        }

        public static AssetBundleManager Singleton { get; private set; }

        [SerializeField]
        protected string serverUrl = "http://localhost/AssetBundles";
        [SerializeField]
        protected string localFolderPath = "AssetBundles/";
        [SerializeField]
        protected string mainAssetBundleName = "init";
        [SerializeField]
        protected string initSceneName = "init";
        [SerializeField]
        protected LoadMode loadMode = LoadMode.FromServerUrl;
        [SerializeField]
        protected LoadMode editorLoadMode = LoadMode.FromLocalPath;
        [SerializeField]
        protected bool loadAssetBundleOnStart = true;
        [SerializeField]
        protected AssetBundleSetting androidSetting = new AssetBundleSetting()
        {
            platformFolderName = "Android",
        };
        [SerializeField]
        protected AssetBundleSetting iosSetting = new AssetBundleSetting()
        {
            platformFolderName = "iOS",
        };
        [SerializeField]
        protected AssetBundleSetting windowsSetting = new AssetBundleSetting()
        {
            platformFolderName = "StandaloneWindows64",
        };
        [SerializeField]
        protected AssetBundleSetting osxSetting = new AssetBundleSetting()
        {
            platformFolderName = "StandaloneOSXIntel64",
        };
        [SerializeField]
        protected AssetBundleSetting linuxSetting = new AssetBundleSetting()
        {
            platformFolderName = "StandaloneLinux64",
        };
        [SerializeField]
        protected AssetBundleSetting serverSetting = new AssetBundleSetting()
        {
            platformFolderName = "StandaloneLinux64",
            overrideLoadMode = LoadMode.FromLocalPath,
        };
        public UnityEvent onManifestLoaded = new UnityEvent();
        public UnityEvent onManifestLoadedFail = new UnityEvent();
        public UnityEvent onDependenciesLoaded = new UnityEvent();
        public UnityEvent onDependenciesLoadedFail = new UnityEvent();
        public UnityEvent onMainAssetBundleLoaded = new UnityEvent();
        public UnityEvent onMainAssetBundleLoadedFail = new UnityEvent();

        public LoadMode CurrentLoadMode
        {
            get
            {
                LoadMode currentLoadMode = loadMode;
                if (Application.isEditor)
                    currentLoadMode = editorLoadMode;
                else if (CurrentSetting.overrideLoadMode != LoadMode.None)
                    currentLoadMode = CurrentSetting.overrideLoadMode;
                return currentLoadMode;
            }
        }
        public LoadState CurrentLoadState { get; protected set; } = LoadState.None;
        public int LoadingDependenciesCount { get; protected set; } = 0;
        public int LoadedDependenciesCount { get; protected set; } = 0;
        public float TotalLoadProgress { get { return (float)LoadedDependenciesCount / (float)(LoadingDependenciesCount + 1); } }
        public string LoadingAssetBundleFileName { get; protected set; }
        public AssetBundleSetting CurrentSetting { get; protected set; }
        public string ServerUrl { get { return !string.IsNullOrEmpty(CurrentSetting.overrideServerUrl) ? CurrentSetting.overrideServerUrl : serverUrl; } }
        public string LocalFolderPath { get { return !string.IsNullOrEmpty(CurrentSetting.overrideLocalFolderPath) ? CurrentSetting.overrideLocalFolderPath : localFolderPath; } }
        public Dictionary<string, AssetBundle> Dependencies { get; private set; } = new Dictionary<string, AssetBundle>();
        public AssetBundle MainAssetBundle { get; protected set; }
        public UnityWebRequest CurrentWebRequest { get; protected set; }

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            Singleton = this;
            DontDestroyOnLoad(gameObject);
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                CurrentSetting = serverSetting;
            }
            else
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        CurrentSetting = androidSetting;
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        CurrentSetting = iosSetting;
                        break;
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                        CurrentSetting = windowsSetting;
                        break;
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                        CurrentSetting = osxSetting;
                        break;
                    case RuntimePlatform.LinuxEditor:
                    case RuntimePlatform.LinuxPlayer:
                        CurrentSetting = linuxSetting;
                        break;
                }
            }
        }

        private void Start()
        {
            if (loadAssetBundleOnStart)
                LoadAssetBundle();
        }

        public void LoadAssetBundle()
        {
            switch (CurrentLoadMode)
            {
                case LoadMode.FromServerUrl:
                    LoadAssetBundleFromServer();
                    break;
                case LoadMode.FromLocalPath:
                    LoadAssetBundleFromLocalFolder();
                    break;
            }
        }

        public void LoadAssetBundleFromServer()
        {
            StartCoroutine(LoadAssetBundleFromUrlRoutine(ServerUrl));
        }

        public void LoadAssetBundleFromLocalFolder()
        {
            StartCoroutine(LoadAssetBundleFromUrlRoutine($"file:///{Path.GetFullPath(".")}/{LocalFolderPath}"));
        }

        private bool IsWebRequestLoadedFail(UnityEvent evt)
        {
            if (CurrentWebRequest.isNetworkError || CurrentWebRequest.isHttpError)
            {
                OnAssetBundleLoadedFail(evt);
                return true;
            }
            return false;
        }

        private void OnAssetBundleLoadedFail(UnityEvent evt)
        {
            evt.Invoke();
            CurrentLoadState = LoadState.None;
        }

        private IEnumerator LoadAssetBundleFromUrlRoutine(string url)
        {
            Dependencies.Clear();

            CurrentLoadState = LoadState.LoadManifest;
            CurrentWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(new Uri(url).Append(CurrentSetting.platformFolderName, CurrentSetting.platformFolderName).AbsoluteUri);
            yield return CurrentWebRequest.SendWebRequest();
            if (IsWebRequestLoadedFail(onManifestLoadedFail))
                yield break;
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(CurrentWebRequest);
            if (assetBundle == null)
            {
                OnAssetBundleLoadedFail(onManifestLoadedFail);
                yield break;
            }
            onManifestLoaded.Invoke();

            CurrentLoadState = LoadState.LoadDependencies;
            AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] dependencies = manifest.GetAllDependencies(mainAssetBundleName);
            LoadedDependenciesCount = 0;
            LoadingDependenciesCount = dependencies.Length;
            foreach (var dependency in dependencies)
            {
                LoadingAssetBundleFileName = dependency;
                CurrentWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(new Uri(url).Append(CurrentSetting.platformFolderName, dependency).AbsoluteUri, Hash128.Compute(dependency));
                yield return CurrentWebRequest.SendWebRequest();
                if (IsWebRequestLoadedFail(onDependenciesLoadedFail))
                    yield break;
                assetBundle = DownloadHandlerAssetBundle.GetContent(CurrentWebRequest);
                if (assetBundle == null)
                {
                    OnAssetBundleLoadedFail(onDependenciesLoadedFail);
                    yield break;
                }
                Dependencies[dependency] = assetBundle;
                LoadedDependenciesCount++;
            }
            onDependenciesLoaded.Invoke();

            CurrentLoadState = LoadState.LoadMainAssetBundle;
            LoadingAssetBundleFileName = mainAssetBundleName;
            CurrentWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(new Uri(url).Append(CurrentSetting.platformFolderName, mainAssetBundleName).AbsoluteUri, Hash128.Compute(mainAssetBundleName));
            yield return CurrentWebRequest.SendWebRequest();
            if (IsWebRequestLoadedFail(onMainAssetBundleLoadedFail))
                yield break;
            assetBundle = DownloadHandlerAssetBundle.GetContent(CurrentWebRequest);
            if (assetBundle == null)
            {
                OnAssetBundleLoadedFail(onMainAssetBundleLoadedFail);
                yield break;
            }
            MainAssetBundle = assetBundle;
            LoadedDependenciesCount++;
            onMainAssetBundleLoaded.Invoke();

            CurrentLoadState = LoadState.Done;
            SceneManager.LoadScene(initSceneName);
        }
    }
}
