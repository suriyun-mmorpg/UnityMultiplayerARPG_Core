using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UISelectionEntry<T> : UIBase
{
    [Header("UI Selection Elements")]
    public GameObject objectSelected;
    public T data;
    public UISelectionManager selectionManager;
    public void OnClickSelect()
    {
        if (selectionManager != null)
            selectionManager.Select(this);
    }

    public void Select()
    {
        if (objectSelected != null)
            objectSelected.SetActive(true);
    }

    public void Deselect()
    {
        if (objectSelected != null)
            objectSelected.SetActive(false);
    }
}
