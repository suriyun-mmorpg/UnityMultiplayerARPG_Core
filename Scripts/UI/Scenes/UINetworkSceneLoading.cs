using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UINetworkSceneLoading : MonoBehaviour
    {
        public static UINetworkSceneLoading Singleton { get; private set; }
        public GameObject rootObject;
        public TextWrapper uiTextProgress;
        public Image imageGage;
        public Slider sliderGage;
        [Tooltip("Delay before deactivate `rootObject`")]
        public float finishedDelay = 0.25f;

        protected virtual void Awake()
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

        public virtual void OnLoadSceneStart(string sceneName, bool isAdditive, bool isOnline, float progress)
        {
            if (rootObject != null)
                rootObject.SetActive(true);
            if (uiTextProgress != null)
                uiTextProgress.text = "0.00%";
            if (imageGage != null)
                imageGage.fillAmount = 0;
            if (sliderGage != null)
                sliderGage.value = 0;
        }

        public virtual void OnLoadSceneProgress(string sceneName, bool isAdditive, bool isOnline, float progress)
        {
            if (uiTextProgress != null)
                uiTextProgress.text = (progress * 100f).ToString("N2") + "%";
            if (imageGage != null)
                imageGage.fillAmount = progress;
            if (sliderGage != null)
                sliderGage.value = progress;
        }

        public virtual void OnLoadSceneFinish(string sceneName, bool isAdditive, bool isOnline, float progress)
        {
            StartCoroutine(OnLoadSceneFinishRoutine());
        }

        protected virtual IEnumerator OnLoadSceneFinishRoutine()
        {
            if (uiTextProgress != null)
                uiTextProgress.text = "100.00%";
            if (imageGage != null)
                imageGage.fillAmount = 1;
            if (sliderGage != null)
                sliderGage.value = 1;
            yield return new WaitForSecondsRealtime(finishedDelay);
            if (rootObject != null)
                rootObject.SetActive(false);
        }
    }
}