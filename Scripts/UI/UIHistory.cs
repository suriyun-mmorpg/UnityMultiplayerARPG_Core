using System.Collections.Generic;
using UnityEngine;

public class UIHistory : MonoBehaviour
{
    public UIBase firstUI;
    protected readonly Stack<UIBase> _uiStack = new Stack<UIBase>();

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
        if (_uiStack.Count > 0)
            _uiStack.Peek().Hide();
        else if (firstUI != null)
            firstUI.Hide();

        _uiStack.Push(ui);
        ui.Show();
    }

    public void Back()
    {
        // Remove current ui from stack
        if (_uiStack.Count > 0)
        {
            UIBase ui = _uiStack.Pop();
            ui.Hide();
        }
        // Show recent ui
        if (_uiStack.Count > 0)
            _uiStack.Peek().Show();
        else if (firstUI != null)
            firstUI.Show();
    }

    public void ClearHistory()
    {
        while (_uiStack.Count > 0)
        {
            UIBase ui = _uiStack.Pop();
            ui.Hide();
        }
        _uiStack.Clear();
        if (firstUI != null)
            firstUI.Show();
    }
}
