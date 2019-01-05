using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIList : MonoBehaviour
{
    public GameObject uiPrefab;
    public Transform uiContainer;
    protected readonly List<GameObject> uis = new List<GameObject>();

    public void Generate<T>(IEnumerable<T> list, System.Action<int, T, GameObject> onGenerateEntry)
    {
        if (uiPrefab == null)
            return;

        int i = 0;
        foreach (T entry in list)
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
                onGenerateEntry.Invoke(i, entry, ui);
            ++i;
        }
        for (; i < uis.Count; ++i)
        {
            GameObject ui = uis[i];
            ui.SetActive(false);
        }
    }

    public void HideAll()
    {
        for (int i = 0; i < uis.Count; ++i)
        {
            GameObject ui = uis[i];
            ui.SetActive(false);
        }
    }
}
