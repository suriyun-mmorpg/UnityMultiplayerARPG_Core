using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum UISelectionMode
{
    SelectSingle,
    Toggle,
}

public abstract class UISelectionManager : MonoBehaviour
{
    public UISelectionMode selectionMode;
    public abstract object GetSelectedUI();
    public abstract void Select(object ui);
    public abstract void Deselect(object ui);
    public abstract void DeselectAll();
    public abstract void DeselectSelectedUI();
    public abstract bool Contains(object ui);
}

public abstract class UISelectionManager<TData, TUI, TEvent> : UISelectionManager
    where TUI : UISelectionEntry<TData>
    where TEvent : UnityEvent<TUI>
{
    public TEvent eventOnSelect;
    public TEvent eventOnSelected;
    public TEvent eventOnDeselect;
    public TEvent eventOnDeselected;

    protected readonly List<TUI> uis = new List<TUI>();
    public TUI SelectedUI { get; protected set; }

    public void Add(TUI ui)
    {
        if (ui == null)
            return;

        ui.selectionManager = this;
        // Select first ui
        if (uis.Count == 0 && selectionMode == UISelectionMode.Toggle)
            Select(ui);
        else
            ui.Deselect();

        uis.Add(ui);
    }

    public int IndexOf(TUI ui)
    {
        return uis.IndexOf(ui);
    }

    public TUI Get(int index)
    {
        return uis[index];
    }

    public bool Remove(TUI ui)
    {
        return uis.Remove(ui);
    }

    public int Count
    {
        get { return uis.Count; }
    }

    public void Clear()
    {
        uis.Clear();
        SelectedUI = null;
    }

    public override sealed object GetSelectedUI()
    {
        return SelectedUI;
    }

    public override sealed void Select(object ui)
    {
        if (ui == null)
            return;
        
        var castedUI = (TUI)ui;
        castedUI.Select();

        if (eventOnSelect != null)
            eventOnSelect.Invoke(castedUI);

        SelectedUI = castedUI;
        foreach (var deselectUI in uis)
        {
            if (deselectUI != castedUI)
                deselectUI.Deselect();
        }

        if (eventOnSelected != null)
            eventOnSelected.Invoke(castedUI);
    }

    public override sealed void Deselect(object ui)
    {
        var castedUI = (TUI)ui;

        if (eventOnDeselect != null)
            eventOnDeselect.Invoke(castedUI);

        SelectedUI = null;
        castedUI.Deselect();

        if (eventOnDeselected != null)
            eventOnDeselected.Invoke(castedUI);
    }

    public override sealed void DeselectAll()
    {
        SelectedUI = null;
        foreach (var deselectUI in uis)
        {
            deselectUI.Deselect();
        }
    }

    public override sealed void DeselectSelectedUI()
    {
        if (SelectedUI != null)
            Deselect(SelectedUI);
    }

    public override bool Contains(object ui)
    {
        return ui is TUI && uis.Contains(ui as TUI);
    }
}
