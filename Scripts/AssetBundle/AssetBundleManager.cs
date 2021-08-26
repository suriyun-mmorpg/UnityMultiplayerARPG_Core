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

        private bool tempErrorOccuring = false;
        private AssetBundle tempAssetBundle = null;

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
            StartCoroutine(LoadAssetBundlesFromUrlRoutine(ServerUrl));
        }

        public void LoadAssetBundleFromLocalFolder()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    StartCoroutine(LoadAssetBundlesFromUrlRoutine($"file:///{Path.GetFullPath(".")}/{LocalFolderPath}"));
                    break;
                default:
                    StartCoroutine(LoadAssetBundlesFromUrlRoutine($"{Path.GetFullPath(".")}/{LocalFolderPath}"));
                    break;
            }
        }

        private bool IsWebRequestLoadedFail(string key, UnityEvent evt)
        {
            if (CurrentWebRequest.isNetworkError || CurrentWebRequest.isHttpError)
            {
                OnAssetBundleLoadedFail(key, evt);
                return true;
            }
            return false;
        }

        private void OnAssetBundleLoadedFail(string key, UnityEvent evt, string error = "")
        {
            tempErrorOccuring = true;
            evt.Invoke();
            CurrentLoadState = LoadState.None;
            string logError = error;
            if (!string.IsNullOrEmpty(CurrentWebRequest.error))
                logError = CurrentWebRequest.error;
            Debug.LogError($"[AssetBundleManager] Load {key} from {CurrentWebRequest.url}, error: {logError}");
        }

        private IEnumerator LoadAssetBundleFromUrlRoutine(string url, string loadKey, UnityEvent successEvt, UnityEvent errorEvt, bool checkAssetFileHash = true)
        {
            Debug.Log($"[AssetBundleManager] Load {loadKey}");
            // Load manifest file to read CRC
            CurrentWebRequest = UnityWebRequest.Get($"{url}.manifest");
            yield return CurrentWebRequest.SendWebRequest();
            if (IsWebRequestLoadedFail(loadKey, errorEvt))
                yield break;
            // Read CRC
            bool foundCRC = false;
            bool foundAssetFileHashKey = false;
            bool foundAssetFileHash = false;
            uint crc = 0;
            string assetFileHash = string.Empty;
            string downloadedText = CurrentWebRequest.downloadHandler.text;
            if (downloadedText.Contains("CRC"))
            {
                string[] splitedLines = downloadedText.Split('\n');
                foreach (string splitedLine in splitedLines)
                {
                    string[] splitedKeyValue = splitedLine.Split(':');
                    if (splitedKeyValue.Length >= 2 && splitedKeyValue[0].Contains("CRC") && uint.TryParse(splitedKeyValue[1].Trim(), out crc))
                        foundCRC = true;
                    if (checkAssetFileHash)
                    {
                        if (splitedKeyValue.Length >= 1 && splitedKeyValue[0].Contains("AssetFileHash"))
                            foundAssetFileHashKey = true;
                        if (foundAssetFileHashKey && splitedKeyValue.Length >= 2 && splitedKeyValue[0].Contains("Hash"))
                        {
                            assetFileHash = splitedKeyValue[1].Trim();
                            foundAssetFileHash = true;
                        }
                    }
                    if (foundCRC && (!checkAssetFileHash || foundAssetFileHash))
                        break;
                }
            }
            // Can't read CRC
            if (!foundCRC)
            {
                OnAssetBundleLoadedFail(loadKey, errorEvt, "No CRC in manifest");
                yield break;
            }
            // Can't read asset hash
            if (checkAssetFileHash && !foundAssetFileHash)
            {
                OnAssetBundleLoadedFail(loadKey, errorEvt, "No asset file hash in manifest");
                yield break;
            }
            // Download asset bundle from server or from cache by CRC
            CurrentWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, Hash128.Compute(assetFileHash), crc);
            yield return CurrentWebRequest.SendWebRequest();
            if (IsWebRequestLoadedFail(loadKey, errorEvt))
                yield break;
            tempAssetBundle = DownloadHandlerAssetBundle.GetContent(CurrentWebRequest);
            if (tempAssetBundle == null)
            {
                OnAssetBundleLoadedFail(loadKey, errorEvt, "No Asset Bundle");
                yield break;
            }
            successEvt.Invoke();
            Debug.Log($"[AssetBundleManager] Load {loadKey} done");
        }

        private IEnumerator LoadAssetBundlesFromUrlRoutine(string url)
        {
            Dependencies.Clear();

            tempErrorOccuring = false;
            tempAssetBundle = null;

            CurrentLoadState = LoadState.LoadManifest;
            yield return StartCoroutine(LoadAssetBundleFromUrlRoutine(new Uri(url).Append(CurrentSetting.platformFolderName, CurrentSetting.platformFolderName).AbsoluteUri, "manifest", onManifestLoaded, onManifestLoadedFail, false));
            if (tempErrorOccuring)
                yield break;

            CurrentLoadState = LoadState.LoadDependencies;
            AssetBundleManifest manifest = tempAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] dependencies = manifest.GetAllDependencies(mainAssetBundleName);
            LoadedDependenciesCount = 0;
            LoadingDependenciesCount = dependencies.Length;
            foreach (var dependency in dependencies)
            {
                LoadingAssetBundleFileName = dependency;
                yield return StartCoroutine(LoadAssetBundleFromUrlRoutine(new Uri(url).Append(CurrentSetting.platformFolderName, dependency).AbsoluteUri, $"dependency: {dependency}", onDependenciesLoaded, onDependenciesLoadedFail));
                if (tempErrorOccuring)
                    yield break;
                Dependencies[dependency] = tempAssetBundle;
                LoadedDependenciesCount++;
            }

            CurrentLoadState = LoadState.LoadMainAssetBundle;
            LoadingAssetBundleFileName = mainAssetBundleName;
            yield return StartCoroutine(LoadAssetBundleFromUrlRoutine(new Uri(url).Append(CurrentSetting.platformFolderName, mainAssetBundleName).AbsoluteUri, "main asset bundle", onMainAssetBundleLoaded, onMainAssetBundleLoadedFail));
            if (tempErrorOccuring)
                yield break;
            MainAssetBundle = tempAssetBundle;
            LoadedDependenciesCount++;

            CurrentLoadState = LoadState.Done;
            SceneManager.LoadScene(initSceneName);
        }
    }
}
