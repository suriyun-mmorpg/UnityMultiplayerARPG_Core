using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UISceneLoading : MonoBehaviour
    {
        public static UISceneLoading Singleton { get; private set; }
        public GameObject rootObject;
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

        public Coroutine LoadScene(string sceneName)
        {
            return StartCoroutine(LoadSceneRoutine(sceneName));
        }

        IEnumerator LoadSceneRoutine(string sceneName)
        {
            if (rootObject != null)
                rootObject.SetActive(true);
            if (uiTextProgress != null)
                uiTextProgress.text = "0.00%";
            if (imageGage != null)
                imageGage.fillAmount = 0;
            yield return null;
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!asyncOp.isDone)
            {
                if (uiTextProgress != null)
                    uiTextProgress.text = (asyncOp.progress * 100f).ToString("N2") + "%";
                if (imageGage != null)
                    imageGage.fillAmount = asyncOp.progress;
                yield return null;
            }
            yield return null;
            if (uiTextProgress != null)
                uiTextProgress.text = "100.00%";
            if (imageGage != null)
                imageGage.fillAmount = 1;
            yield return new WaitForSecondsRealtime(0.25f);
            if (rootObject != null)
                rootObject.SetActive(false);
        }
    }
}
