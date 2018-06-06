using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIBase : MonoBehaviour
{
    public bool hideOnAwake = false;
    public bool moveToLastSiblingOnShow = false;
    public GameObject root;
    public UnityEvent onShow;
    public UnityEvent onHide;

    private bool isAwaken;

    public GameObject CacheRoot
    {
        get
        {
            if (root == null)
                root = gameObject;
            return root;
        }
    }

    private Canvas cacheRootCanvas;
    public Canvas CacheRootCanvas
    {
        get
        {
            if (cacheRootCanvas == null)
                cacheRootCanvas = CacheRoot.GetComponent<Canvas>();
            if (cacheRootCanvas == null)
                cacheRootCanvas = CacheRoot.AddComponent<Canvas>();
            return cacheRootCanvas;
        }
    }

    private GraphicRaycaster cacheGraphicRaycaster;
    public GraphicRaycaster CacheGraphicRaycaster
    {
        get
        {
            if (cacheGraphicRaycaster == null)
                cacheGraphicRaycaster = CacheRoot.GetComponent<GraphicRaycaster>();
            if (cacheGraphicRaycaster == null)
                cacheGraphicRaycaster = CacheRoot.AddComponent<GraphicRaycaster>();
            return cacheGraphicRaycaster;
        }
    }

    protected virtual void Awake()
    {
        if (isAwaken)
            return;
        isAwaken = true;

        if (hideOnAwake)
            Hide();
        else
            Show();
    }

    public virtual bool IsVisible()
    {
        return CacheRoot.activeSelf && CacheRootCanvas.enabled;
    }

    public virtual void Show()
    {
        isAwaken = true;
        CacheRootCanvas.enabled = true;
        CacheGraphicRaycaster.enabled = true;
        if (!CacheRoot.activeSelf)
            CacheRoot.SetActive(true);
        onShow.Invoke();
        if (moveToLastSiblingOnShow)
            CacheRoot.transform.SetAsLastSibling();
    }

    public virtual void Hide()
    {
        isAwaken = true;
        CacheRootCanvas.enabled = false;
        CacheGraphicRaycaster.enabled = false;
        CacheRoot.SetActive(false);
        onHide.Invoke();
    }

    public void Toggle()
    {
        if (IsVisible())
            Hide();
        else
            Show();
    }
}
