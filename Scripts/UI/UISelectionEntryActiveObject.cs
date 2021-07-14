using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UISelectionEntryActiveObject : MonoBehaviour
{
    [System.Serializable]
    public struct Setting
    {
        public GameObject defaultObject;
        public GameObject selectedObject;
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
            setting.defaultObject.SetActive(true);
            setting.selectedObject.SetActive(false);
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
                setting.defaultObject.SetActive(!dirtySelected);
                setting.selectedObject.SetActive(dirtySelected);
            }
        }
    }

    [ContextMenu("Set Active Default Object")]
    public void SetActiveDefaultObject()
    {
#if UNITY_EDITOR
        for (int i = 0; i < settings.Length; ++i)
        {
            Setting setting = settings[i];
            setting.defaultObject.SetActive(true);
            setting.selectedObject.SetActive(false);
            settings[i] = setting;
        }
        EditorUtility.SetDirty(this);
#endif
    }


    [ContextMenu("Swap Default Object and Selected Object")]
    public void SwapDefaultObjectAndSelectedObject()
    {
#if UNITY_EDITOR
        for (int i = 0; i < settings.Length; ++i)
        {
            Setting setting = settings[i];
            GameObject defaultObject = setting.defaultObject;
            GameObject selectedObject = setting.selectedObject;
            setting.defaultObject = selectedObject;
            setting.selectedObject = defaultObject;
            settings[i] = setting;
        }
        EditorUtility.SetDirty(this);
#endif
    }
}
