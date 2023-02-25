using System.Collections.Generic;
using UnityEngine;

public class UIStackManager : MonoBehaviour
{
    public string closeButtonName = "CloseUI";
    private static Stack<UIStackEntry> entries = new Stack<UIStackEntry>();

    private void Awake()
    {
        Clear();
    }

    private void Update()
    {
        if (InputManager.GetButtonDown(closeButtonName))
        {
            UIStackEntry entry;
            while (entries.Count > 0)
            {
                entry = entries.Pop();
                if (entry != null)
                {
                    entry.Hide();
                    break;
                }
            }
        }
    }

    public void Clear()
    {
        entries.Clear();
    }

    public static void Add(UIStackEntry entry)
    {
        entries.Push(entry);
    }
}
