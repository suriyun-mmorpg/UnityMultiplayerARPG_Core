using UnityEngine;

public abstract class UISelectionEntry<T> : UIBase, IUISelectionEntry
{
    [Header("UI Selection Elements")]
    public GameObject objectSelected;
    private T data;
    public T Data
    {
        get { return data; }
        set
        {
            data = value;
            ForceUpdate();
        }
    }
    public UISelectionManager selectionManager;
    public float updateUIRepeatRate = 0.5f;
    protected float _updateCountDown;
    private bool _isSelected;
    public bool IsSelected
    {
        get { return _isSelected; }
        protected set
        {
            _isSelected = value;
            if (objectSelected != null)
                objectSelected.SetActive(value);
        }
    }

    public System.Action onUpdateUI;
    public System.Action<T> onUpdateData;

    protected override void Awake()
    {
        base.Awake();
        IsSelected = false;
        _updateCountDown = 0f;
    }

    protected virtual void OnEnable()
    {
        UpdateUI();
    }

    protected virtual void OnDisable()
    {
        _updateCountDown = 0f;
    }

    protected virtual void Update()
    {
        _updateCountDown -= Time.deltaTime;
        if (_updateCountDown <= 0f)
        {
            _updateCountDown = updateUIRepeatRate;
            UpdateUI();
            if (onUpdateUI != null)
                onUpdateUI.Invoke();
            this.InvokeInstanceDevExtMethods("UpdateUI");
        }
    }

    public void ForceUpdate()
    {
        UpdateData();
        UpdateUI();
        if (onUpdateData != null)
            onUpdateData.Invoke(Data);
        this.InvokeInstanceDevExtMethods("UpdateData");
    }

    public void OnClickSelect()
    {
        if (selectionManager != null)
        {
            UISelectionMode selectionMode = selectionManager.selectionMode;
            if (selectionMode != UISelectionMode.Toggle && IsSelected)
                selectionManager.Deselect(this);
            else if (!IsSelected)
                selectionManager.Select(this);
        }
    }

    public void Select()
    {
        IsSelected = true;
    }

    public void Deselect()
    {
        IsSelected = false;
    }

    public void SetData(object data)
    {
        if (data is T)
            Data = (T)data;
    }

    public object GetData()
    {
        return Data;
    }

    protected virtual void UpdateUI() { }
    protected abstract void UpdateData();
}
