using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    protected virtual void Awake()
    {
        if (isAwaken)
            return;
        isAwaken = true;

        if (hideOnAwake)
            Hide();
    }

    public virtual bool IsVisible()
    {
        return CacheRoot.activeSelf;
    }

    public virtual void Show()
    {
        isAwaken = true;
        CacheRoot.SetActive(true);
        onShow.Invoke();
        if (moveToLastSiblingOnShow)
            CacheRoot.transform.SetAsLastSibling();
    }

    public virtual void Hide()
    {
        isAwaken = true;
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
