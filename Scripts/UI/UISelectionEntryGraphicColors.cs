using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UISelectionEntryGraphicColors : MonoBehaviour
{
    [System.Serializable]
    public struct Setting
    {
        public Graphic graphic;
        public Color defaultColor;
        public Color selectedColor;
    }

    public Setting[] settings;
    private IUISelectionEntry entry;
    private bool dirtySelected;

    private void Awake()
    {
        entry = GetComponent<IUISelectionEntry>();
    }

    private void OnEnable()
    {
        dirtySelected = false;
        foreach (Setting setting in settings)
        {
            setting.graphic.color = setting.defaultColor;
        }
    }

    private void Update()
    {
        if (entry == null)
            return;

        if (dirtySelected != entry.IsSelected)
        {
            dirtySelected = entry.IsSelected;
            foreach (Setting setting in settings)
            {
                setting.graphic.color = dirtySelected ? setting.selectedColor : setting.defaultColor;
            }
        }
    }

    [ContextMenu("Set default color by graphic's color")]
    public void SetDefaultColorByGraphic()
    {
#if UNITY_EDITOR
        for (int i = 0; i < settings.Length; ++i)
        {
            Setting setting = settings[i];
            setting.defaultColor = setting.graphic.color;
            settings[i] = setting;
        }
        EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Swap default color and selected color")]
    public void SwapDefaultColorAndSelectedColor()
    {
#if UNITY_EDITOR
        for (int i = 0; i < settings.Length; ++i)
        {
            Setting setting = settings[i];
            Color defaultColor = setting.defaultColor;
            Color selectedColor = setting.selectedColor;
            setting.defaultColor = selectedColor;
            setting.selectedColor = defaultColor;
            settings[i] = setting;
        }
        EditorUtility.SetDirty(this);
#endif
    }
}
