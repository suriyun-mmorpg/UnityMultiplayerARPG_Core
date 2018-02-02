using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class UISelectionManager : MonoBehaviour
{
    public abstract void Select(object ui);
}

public abstract class UISelectionManager<TData, TUI, TEvent> : UISelectionManager
    where TUI : UISelectionEntry<TData>
    where TEvent : UnityEvent<TUI>
{
    public TEvent eventOnSelect;

    protected readonly List<TUI> uis = new List<TUI>();
    public TUI SelectedUI { get; protected set; }

    public void Add(TUI ui)
    {
        if (ui == null)
            return;

        ui.selectionManager = this;
        // Select first ui
        if (uis.Count == 0)
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
}
