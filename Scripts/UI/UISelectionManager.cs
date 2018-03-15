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
}

public abstract class UISelectionManager<TData, TUI, TEvent> : UISelectionManager
    where TUI : UISelectionEntry<TData>
    where TEvent : UnityEvent<TUI>
{
    public TEvent eventOnSelect;
    public TEvent eventOnDeselect;

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
    }

    public override sealed void Deselect(object ui)
    {
        var castedUI = (TUI)ui;

        if (eventOnDeselect != null)
            eventOnDeselect.Invoke(castedUI);

        SelectedUI = null;
        castedUI.Deselect();
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
}
