using LiteNetLibManager;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIAddressableAssetDownloadManager : AddressableAssetDownloadManager
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
            onDownloadedAll.AddListener(OnDownloadedAll);
        }

        private void OnDestroy()
        {
            onStart.RemoveAllListeners();
            onStart = null;
            onEnd.RemoveAllListeners();
            onEnd = null;
            onDownloadedAll.RemoveAllListeners();
            onDownloadedAll = null;
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

        private void OnDownloadedAll()
        {
            UpdateTotalProgress();
        }

        private void UpdateTotalProgress()
        {
            if (uiTextTotalProgress != null)
            {
                uiTextTotalProgress.gameObject.SetActive(true);
                uiTextTotalProgress.text = string.Format(formatLoadedFilesProgress.Text, LoadedCount, TotalCount);
            }
        }

        protected override void OnFileSizeRetrieving()
        {
            base.OnFileSizeRetrieving();
            if (loadGage != null)
                loadGage.fillAmount = 0f;
            if (loadGageRoot != null)
                loadGageRoot.SetActive(false);
            if (uiTextProgress != null)
                uiTextProgress.gameObject.SetActive(true);
            uiTextStatus.text = msgGetFileSize.Text;
        }

        protected override void OnDepsFileDownloading(long downloadSize, long fileSize, float percentComplete)
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
            UpdateTotalProgress();
        }

        protected override void OnDepsDownloading()
        {
            base.OnDepsDownloading();
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

        protected override void OnDepsDownloaded()
        {
            base.OnDepsDownloaded();
            if (loadGage != null)
                loadGage.fillAmount = 1f;
            if (loadGageRoot != null)
                loadGageRoot.SetActive(false);
            if (uiTextProgress != null)
                uiTextProgress.gameObject.SetActive(false);
            if (uiTextStatus != null)
                uiTextStatus.text = msgFileLoaded.Text;
            UpdateTotalProgress();
        }
    }
}
