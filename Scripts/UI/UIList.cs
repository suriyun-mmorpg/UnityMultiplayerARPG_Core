using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIList : MonoBehaviour
{
    public GameObject uiPrefab;
    public Transform uiContainer;
    protected readonly List<GameObject> uis = new List<GameObject>();

    public void MakeList<T>(List<T> list, System.Action<T, GameObject> onCreate)
    {
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
            }
            ui.SetActive(true);
            if (onCreate != null)
                onCreate(list[i], ui);
        }
        for (; i < uis.Count; ++i)
        {
            var ui = uis[i];
            ui.gameObject.SetActive(false);
        }
    }
}
