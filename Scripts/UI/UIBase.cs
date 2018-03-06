using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public bool hideOnAwake = false;
    public GameObject root;
    
    public GameObject TempRoot
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
        if (hideOnAwake)
            Hide();
    }

    public virtual bool IsVisible()
    {
        return TempRoot.activeSelf;
    }

    public virtual void Show()
    {
        TempRoot.SetActive(true);
    }

    public virtual void Hide()
    {
        TempRoot.SetActive(false);
    }

    public virtual void Toggle()
    {
        if (IsVisible())
            Hide();
        else
            Show();
    }
}
