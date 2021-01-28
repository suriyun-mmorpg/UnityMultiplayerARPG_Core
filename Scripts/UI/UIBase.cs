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

    public bool AlreadyCachedComponents { get; private set; }

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

    protected virtual void CacheComponents()
    {
        if (AlreadyCachedComponents)
            return;

        if (root == null)
            root = gameObject;
        AlreadyCachedComponents = true;
    }

    public virtual bool IsVisible()
    {
        CacheComponents();
        return CacheRoot.activeSelf;
    }

    public virtual void Show()
    {
        isAwaken = true;
        CacheComponents();
        if (!CacheRoot.activeSelf)
            CacheRoot.SetActive(true);
        if (onShow != null)
            onShow.Invoke();
        if (moveToLastSiblingOnShow)
            CacheRoot.transform.SetAsLastSibling();
        this.InvokeInstanceDevExtMethods("Show");
    }

    public virtual void Hide()
    {
        isAwaken = true;
        CacheComponents();
        CacheRoot.SetActive(false);
        if (onHide != null)
            onHide.Invoke();
        this.InvokeInstanceDevExtMethods("Hide");
    }

    public void SetVisible(bool isVisible)
    {
        if (!isVisible && IsVisible())
            Hide();
        if (isVisible && !IsVisible())
            Show();
    }

    public void Toggle()
    {
        if (IsVisible())
            Hide();
        else
            Show();
    }
}
