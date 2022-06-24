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

        public virtual Coroutine LoadScene(string sceneName)
        {
            return StartCoroutine(LoadSceneRoutine(sceneName));
        }

        protected virtual IEnumerator LoadSceneRoutine(string sceneName)
        {
            if (SceneManager.GetActiveScene().name.Equals(sceneName))
                yield break;
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
            yield return new WaitForSecondsRealtime(finishedDelay);
            if (rootObject != null)
                rootObject.SetActive(false);
        }
    }
}
