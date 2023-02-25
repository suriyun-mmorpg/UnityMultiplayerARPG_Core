using UnityEngine;

[RequireComponent(typeof(UIBase))]
public class UIStackEntry : MonoBehaviour
{
    private UIBase ui;

    private void Awake()
    {
        ui = GetComponent<UIBase>();
    }

    private void OnEnable()
    {
        UIStackManager.Add(this);
    }

    public void Hide()
    {
        ui.Hide();
    }
}
