using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIList : MonoBehaviour
{
    public GameObject uiPrefab;
    public Transform uiContainer;
    protected readonly List<GameObject> uis = new List<GameObject>();

    public void Generate<T>(IList<T> list, System.Action<int, T, GameObject> onGenerateEntry)
    {
        if (uiPrefab == null)
            return;

        var i = 0;
        for (; i < list.Count; ++i)
        {
            GameObject ui;
            if (i < uis.Count)
                ui = uis[i];
            else
            {
                ui = Instantiate(uiPrefab);
                ui.transform.SetParent(uiContainer);
                ui.transform.localScale = Vector3.one;
                ui.transform.SetAsLastSibling();
                uis.Add(ui);
            }
            ui.SetActive(true);
            if (onGenerateEntry != null)
                onGenerateEntry(i, list[i], ui);
        }
        for (; i < uis.Count; ++i)
        {
            var ui = uis[i];
            ui.SetActive(false);
        }
    }

    public void HideAll()
    {
        for (var i = 0; i < uis.Count; ++i)
        {
            GameObject ui = uis[i];
            ui.SetActive(false);
        }
    }
}
