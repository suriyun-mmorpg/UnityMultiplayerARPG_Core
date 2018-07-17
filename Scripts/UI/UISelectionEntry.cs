using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UISelectionEntry<T> : UIBase
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
            UpdateData();
            this.InvokeClassAddOnMethods("UpdateData");
        }
    }
    public UISelectionManager selectionManager;
    public float updateUIRepeatRate = 0.5f;
    protected float lastUpdateTime;

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

    protected override void Awake()
    {
        base.Awake();
        IsSelected = false;
        lastUpdateTime = Time.unscaledTime;
    }

    protected virtual void Update()
    {
        if (Time.unscaledTime - lastUpdateTime >= updateUIRepeatRate)
        {
            UpdateUI();
            this.InvokeClassAddOnMethods("UpdateUI");
            lastUpdateTime = Time.unscaledTime;
        }
    }

    public void ForceUpdate()
    {
        UpdateData();
        this.InvokeClassAddOnMethods("UpdateData");
    }

    public void OnClickSelect()
    {
        if (selectionManager != null)
        {
            var selectionMode = selectionManager.selectionMode;
            var selectedUI = selectionManager.GetSelectedUI();
            if (selectionMode != UISelectionMode.Toggle && selectedUI != null && (UIBase)selectedUI == this)
                selectionManager.Deselect(this);
            else if (selectedUI == null || (UIBase)selectedUI != this)
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
    
    protected virtual void UpdateUI() { }
    protected abstract void UpdateData();
}
