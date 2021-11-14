using System.Collections.Generic;
using UnityEngine;

public class UIHistory : MonoBehaviour
{
    public UIBase firstUI;
    protected readonly Stack<UIBase> uiStack = new Stack<UIBase>();

    private void Awake()
    {
        if (firstUI != null)
            firstUI.Show();
    }

    public void Next(UIBase ui)
    {
        if (ui == null)
            return;
        // Hide latest ui
        if (uiStack.Count > 0)
            uiStack.Peek().Hide();
        else if (firstUI != null)
            firstUI.Hide();

        uiStack.Push(ui);
        ui.Show();
    }

    public void Back()
    {
        // Remove current ui from stack
        if (uiStack.Count > 0)
        {
            UIBase ui = uiStack.Pop();
            ui.Hide();
        }
        // Show recent ui
        if (uiStack.Count > 0)
            uiStack.Peek().Show();
        else if (firstUI != null)
            firstUI.Show();
    }

    public void ClearHistory()
    {
        while (uiStack.Count > 0)
        {
            UIBase ui = uiStack.Pop();
            ui.Hide();
        }
        uiStack.Clear();
        if (firstUI != null)
            firstUI.Show();
    }
}
