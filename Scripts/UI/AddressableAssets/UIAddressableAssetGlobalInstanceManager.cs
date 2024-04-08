using UnityEngine;
using LiteNetLibManager;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIAddressableAssetGlobalInstanceManager : AddressableAssetGlobalInstanceManager
    {
        public GameObject root;
        public TextWrapper uiTextStatus;
        public TextWrapper uiTextProgress;
        public TextWrapper uiTextTotalProgress;
        public GameObject loadGageRoot;
        public Image loadGage;
        public LanguageTextSetting msgGetFileSize = new LanguageTextSetting()
        {
            defaultText = "Downloading...",
        };
        public LanguageTextSetting msgFileLoading = new LanguageTextSetting()
        {
            defaultText = "Downloading...",
        };
        public LanguageTextSetting msgFileLoaded = new LanguageTextSetting();
        public LanguageTextSetting formatFileLoadingProgress = new LanguageTextSetting()
        {
            defaultText = "{0} {1}%",
        };
        public LanguageTextSetting formatLoadedFilesProgress = new LanguageTextSetting()
        {
            defaultText = "{0}/{1}",
        };

        private void Awake()
        {
            onStart.AddListener(OnStart);
            onEnd.AddListener(OnEnd);
            onFileSizeRetrieving.AddListener(OnFileSizeRetrieving);
            onFileSizeRetrieved.AddListener(OnFileSizeRetrieved);
            onDepsDownloading.AddListener(OnDepsDownloading);
            onDepsDownloaded.AddListener(OnDepsDownloaded);
            onDownloading.AddListener(OnDownloading);
            onDownloaded.AddListener(OnDownloaded);
            onFileDownloading.AddListener(OnFileDownloading);
        }

        private void OnDestroy()
        {
            onStart.RemoveAllListeners();
            onStart = null;
            onEnd.RemoveAllListeners();
            onEnd = null;
            onFileSizeRetrieving.RemoveAllListeners();
            onFileSizeRetrieving = null;
            onFileSizeRetrieved.RemoveAllListeners();
            onFileSizeRetrieved = null;
            onDepsDownloading.RemoveAllListeners();
            onDepsDownloading = null;
            onDepsDownloaded.RemoveAllListeners();
            onDepsDownloaded = null;
            onDownloading.RemoveAllListeners();
            onDownloading = null;
            onDownloaded.RemoveAllListeners();
            onDownloaded = null;
            onFileDownloading.RemoveAllListeners();
            onFileDownloading = null;
        }

        private void OnStart()
        {
            if (root != null)
                root.SetActive(true);
        }

        private void OnEnd()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void OnFileSizeRetrieving()
        {
            if (loadGage != null)
                loadGage.fillAmount = 0f;
            if (loadGageRoot != null)
                loadGageRoot.SetActive(false);
            if (uiTextProgress != null)
                uiTextProgress.gameObject.SetActive(false);
            uiTextStatus.text = msgGetFileSize.Text;
        }

        private void OnFileSizeRetrieved(long fileSize)
        {

        }

        private void OnFileDownloading(long downloadedSize, long fileSize, float percentComplete)
        {
            if (uiTextProgress != null)
            {
                if (fileSize > 0)
                    uiTextProgress.text = string.Format(formatFileLoadingProgress.Text, GenericUtils.MinMaxSizeSuffix((long)(percentComplete * fileSize), fileSize), (percentComplete * 100).ToString("N2"));
                else
                    uiTextProgress.text = string.Empty;
            }
            if (loadGage != null)
                loadGage.fillAmount = percentComplete;
        }

        private void OnDepsDownloading(int loadedCount, int totalCount)
        {
            if (uiTextTotalProgress != null)
            {
                uiTextTotalProgress.gameObject.SetActive(true);
                uiTextTotalProgress.text = string.Format(formatLoadedFilesProgress.Text, loadedCount, totalCount);
            }
            if (uiTextStatus != null)
                uiTextStatus.text = msgFileLoading.Text;
            if (uiTextProgress != null)
            {
                uiTextProgress.gameObject.SetActive(true);
                uiTextProgress.text = string.Empty;
            }
            if (loadGageRoot != null)
                loadGageRoot.SetActive(true);
        }

        private void OnDepsDownloaded(int loadedCount, int totalCount)
        {
            if (uiTextTotalProgress != null)
                uiTextTotalProgress.gameObject.SetActive(false);
            if (loadGage != null)
                loadGage.fillAmount = 1f;
            if (loadGageRoot != null)
                loadGageRoot.SetActive(false);
            if (uiTextProgress != null)
                uiTextProgress.gameObject.SetActive(false);
            if (uiTextStatus != null)
                uiTextStatus.text = msgFileLoaded.Text;
        }

        private void OnDownloading(int loadedCount, int totalCount)
        {
            if (uiTextTotalProgress != null)
            {
                uiTextTotalProgress.gameObject.SetActive(true);
                uiTextTotalProgress.text = string.Format(formatLoadedFilesProgress.Text, loadedCount, totalCount);
            }
            if (uiTextStatus != null)
                uiTextStatus.text = msgFileLoading.Text;
            if (uiTextProgress != null)
            {
                uiTextProgress.gameObject.SetActive(true);
                uiTextProgress.text = string.Empty;
            }
            if (loadGageRoot != null)
                loadGageRoot.SetActive(true);
        }

        private void OnDownloaded(int loadedCount, int totalCount)
        {
            if (uiTextTotalProgress != null)
                uiTextTotalProgress.gameObject.SetActive(false);
            if (loadGage != null)
                loadGage.fillAmount = 1f;
            if (loadGageRoot != null)
                loadGageRoot.SetActive(false);
            if (uiTextProgress != null)
                uiTextProgress.gameObject.SetActive(false);
            if (uiTextStatus != null)
                uiTextStatus.text = msgFileLoaded.Text;
        }
    }
}
