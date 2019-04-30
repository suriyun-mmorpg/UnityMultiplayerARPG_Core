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
            ForceUpdate();
        }
    }
    public UISelectionManager selectionManager;
    public float updateUIRepeatRate = 0.5f;
    protected float lastUpdateTime;
    private float tempUpdateTime;
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
        tempUpdateTime = Time.unscaledTime;
        if (tempUpdateTime - lastUpdateTime >= updateUIRepeatRate)
        {
            UpdateUI();
            this.InvokeInstanceDevExtMethods("UpdateUI");
            lastUpdateTime = tempUpdateTime;
        }
    }

    public override void Show()
    {
        UpdateUI();
        base.Show();
    }

    public void ForceUpdate()
    {
        UpdateData();
        UpdateUI();
        this.InvokeInstanceDevExtMethods("UpdateData");
    }

    public void OnClickSelect()
    {
        if (selectionManager != null)
        {
            UISelectionMode selectionMode = selectionManager.selectionMode;
            object selectedUI = selectionManager.GetSelectedUI();
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
