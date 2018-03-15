using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGroup : UIBase
{
    public UIBase[] subSet;

    public override void Show()
    {
        foreach (var entry in subSet)
        {
            entry.Show();
        }
        base.Show();
    }

    public override void Hide()
    {
        foreach (var entry in subSet)
        {
            entry.Hide();
        }
        base.Hide();
    }
}
