using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UIBaseEvent : UnityEvent<UIBase>
{

}

public class UIBase : MonoBehaviour
{
    public bool hideOnAwake = false;
    public bool moveToLastSiblingOnShow = false;
    public GameObject root;
    public UnityEvent onShow = new UnityEvent();
    public UnityEvent onHide = new UnityEvent();
    public UIBaseEvent onShowWithObject = new UIBaseEvent();
    public UIBaseEvent onHideWithObject = new UIBaseEvent();

    private bool isAwaken;

    public GameObject CacheRoot
    {
        get
        {
            CacheComponents();
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
        return CacheRoot.activeSelf;
    }

    public virtual void Show()
    {
        if (IsVisible())
            return;
        isAwaken = true;
        CacheComponents();
        if (!CacheRoot.activeSelf)
            CacheRoot.SetActive(true);
        onShow.Invoke();
        onShowWithObject.Invoke(this);
        OnShow();
        if (moveToLastSiblingOnShow)
            CacheRoot.transform.SetAsLastSibling();
        this.InvokeInstanceDevExtMethods("Show");
    }

    public virtual void OnShow()
    {

    }

    public virtual void Hide()
    {
        if (!IsVisible())
            return;
        isAwaken = true;
        CacheComponents();
        CacheRoot.SetActive(false);
        onHide.Invoke();
        onHideWithObject.Invoke(this);
        OnHide();
        this.InvokeInstanceDevExtMethods("Hide");
    }

    public virtual void OnHide()
    {

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
