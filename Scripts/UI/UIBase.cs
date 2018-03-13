using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public bool hideOnAwake = false;
    public bool moveToLastSiblingOnShow = false;
    public GameObject root;

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
        if (moveToLastSiblingOnShow)
            CacheRoot.transform.SetAsLastSibling();
    }

    public virtual void Hide()
    {
        isAwaken = true;
        CacheRoot.SetActive(false);
    }

    public virtual void Toggle()
    {
        if (IsVisible())
            Hide();
        else
            Show();
    }
}
