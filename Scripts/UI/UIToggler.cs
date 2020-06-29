using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggler : MonoBehaviour
{
    public UIBase ui;
    public KeyCode key;

    private void Update()
    {
        if (Input.GetKeyDown(key))
            ui.Toggle();
    }
}
