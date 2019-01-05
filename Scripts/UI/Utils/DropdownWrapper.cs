using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if USE_TEXT_MESH_PRO
using TMPro;
#endif

public class DropdownWrapper : MonoBehaviour {
    public Dropdown unityDropdown;
#if USE_TEXT_MESH_PRO
    public TMP_Dropdown textMeshDropdown;
#endif

    public bool interactable
    {
        get
        {
            if (unityDropdown != null)
                return unityDropdown.interactable;
#if USE_TEXT_MESH_PRO
            if (textMeshDropdown != null)
                return textMeshDropdown.interactable;
#endif
            return false;
        }
    }

    public int value
    {
        get
        {
            if (unityDropdown != null)
                return unityDropdown.value;
#if USE_TEXT_MESH_PRO
            if (textMeshDropdown != null)
                return textMeshDropdown.value;
#endif
            return 0;
        }

        set
        {
            if (unityDropdown != null)
                unityDropdown.value = value;
#if USE_TEXT_MESH_PRO
            if (textMeshDropdown != null)
                textMeshDropdown.value = value;
#endif
        }
    }

    public virtual List<OptionData> options
    {
        get
        {
            if (unityDropdown != null)
            {
                if (unityDropdown.options == null)
                    return null;
                List<OptionData> options = new List<OptionData>();
                foreach (Dropdown.OptionData entry in unityDropdown.options)
                {
                    options.Add(new OptionData(entry.text, entry.image));
                }
                return options;
            }
#if USE_TEXT_MESH_PRO
            if (textMeshDropdown != null)
            {
                if (textMeshDropdown.options == null)
                    return null;
                var options = new List<OptionData>();
                foreach (var entry in textMeshDropdown.options)
                {
                    options.Add(new OptionData(entry.text, entry.image));
                }
                return options;
            }
#endif
            return null;
        }

        set
        {
            if (unityDropdown != null)
            {
                if (value == null)
                    unityDropdown.options = null;
                else
                {
                    List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
                    foreach (OptionData entry in value)
                    {
                        options.Add(new Dropdown.OptionData(entry.text, entry.image));
                    }
                    unityDropdown.options = options;
                }
            }
#if USE_TEXT_MESH_PRO
            if (textMeshDropdown != null)
            {
                if (value == null)
                    textMeshDropdown.options = null;
                else
                {
                    var options = new List<TMP_Dropdown.OptionData>();
                    foreach (var entry in value)
                    {
                        options.Add(new TMP_Dropdown.OptionData(entry.text, entry.image));
                    }
                    textMeshDropdown.options = options;
                }
            }
#endif
        }
    }

    public virtual UnityEvent<int> onValueChanged
    {
        get
        {
            if (unityDropdown != null) return unityDropdown.onValueChanged;
#if USE_TEXT_MESH_PRO
            if (textMeshDropdown != null) return textMeshDropdown.onValueChanged;
#endif
            return null;
        }

        set
        {
            if (unityDropdown != null) unityDropdown.onValueChanged = value as Dropdown.DropdownEvent;
#if USE_TEXT_MESH_PRO
            if (textMeshDropdown != null) textMeshDropdown.onValueChanged = value as TMP_Dropdown.DropdownEvent;
#endif
        }
    }

    public class OptionData
    {
        public OptionData() : this(string.Empty, null)
        {
        }

        public OptionData(string text) : this(text, null)
        {

        }

        public OptionData(Sprite image) : this(string.Empty, image)
        {

        }

        public OptionData(string text, Sprite image)
        {
            this.text = text;
            this.image = image;
        }


        public string text { get; set; }
        public Sprite image { get; set; }
    }
}
