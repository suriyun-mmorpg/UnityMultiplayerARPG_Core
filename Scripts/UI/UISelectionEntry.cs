using System.Collections;
using System.Collections.Generic;
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
    protected float updateCountDown;
    private bool isSelected;
    public bool IsSelected
    {
        get { return isSelected; }
        protected set
        {
            isSelected = value;
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
        updateCountDown = 0f;
    }

    protected virtual void OnEnable()
    {
        UpdateUI();
    }

    protected virtual void OnDisable()
    {
        updateCountDown = updateUIRepeatRate;
    }

    protected virtual void Update()
    {
        updateCountDown -= Time.deltaTime;
        if (updateCountDown <= 0f)
        {
            updateCountDown = updateUIRepeatRate;
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
