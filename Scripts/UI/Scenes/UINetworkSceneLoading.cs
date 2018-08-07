using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UINetworkSceneLoading : MonoBehaviour
    {
        public static UINetworkSceneLoading Singleton { get; private set; }
        public GameObject rootObject;
        public Text textProgress;
        public TextWrapper uiTextProgress;
        public Image imageGage;

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Singleton = this;

            if (rootObject != null)
                rootObject.SetActive(false);
        }

        public void OnLoadSceneStart(string sceneName, bool isOnline, float progress)
        {
            MigrateUIComponents();
            if (rootObject != null)
                rootObject.SetActive(true);
            if (uiTextProgress != null)
                uiTextProgress.text = "0.00%";
            if (imageGage != null)
                imageGage.fillAmount = 0;
        }

        public void OnLoadSceneProgress(string sceneName, bool isOnline, float progress)
        {
            MigrateUIComponents();
            if (uiTextProgress != null)
                uiTextProgress.text = (progress * 100f).ToString("N2") + "%";
            if (imageGage != null)
                imageGage.fillAmount = progress;
        }

        public void OnLoadSceneFinish(string sceneName, bool isOnline, float progress)
        {
            StartCoroutine(OnLoadSceneFinishRoutine());
        }

        IEnumerator OnLoadSceneFinishRoutine()
        {
            MigrateUIComponents();
            if (uiTextProgress != null)
                uiTextProgress.text = "100.00%";
            if (imageGage != null)
                imageGage.fillAmount = 1;
            yield return new WaitForSecondsRealtime(0.25f);
            if (rootObject != null)
                rootObject.SetActive(false);
        }

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextProgress = UIWrapperHelpers.SetWrapperToText(textProgress, uiTextProgress);
        }
    }
}
