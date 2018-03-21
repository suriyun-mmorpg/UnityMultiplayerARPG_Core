using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISceneLoading : MonoBehaviour
{
    public static UISceneLoading Singleton { get; private set; }
    public GameObject rootObject;
    public Text textProgress;
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
        if (textProgress != null)
            textProgress.text = "0.00%";
        yield return null;
        var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!asyncOp.isDone)
        {
            if (textProgress != null)
                textProgress.text = (asyncOp.progress * 100f).ToString("N2") + "%";
            if (imageGage != null)
                imageGage.fillAmount = asyncOp.progress;
            yield return null;
        }
        yield return null;
        if (textProgress != null)
            textProgress.text = "100.00%";
        if (imageGage != null)
            imageGage.fillAmount = 1;
        yield return new WaitForSecondsRealtime(0.25f);
        if (rootObject != null)
            rootObject.SetActive(false);
    }
}
