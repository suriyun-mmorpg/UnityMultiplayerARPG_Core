using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public bool hideOnAwake = false;
    public GameObject root;

    private bool isAwaken;

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
        if (isAwaken)
            return;
        isAwaken = true;

        if (hideOnAwake)
            Hide();
    }

    public virtual bool IsVisible()
    {
        return TempRoot.activeSelf;
    }

    public virtual void Show()
    {
        isAwaken = true;
        TempRoot.SetActive(true);
    }

    public virtual void Hide()
    {
        isAwaken = true;
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
