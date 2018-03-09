using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterAttributeLevel : UISelectionEntry<int>
{
    public CharacterAttribute attribute;
    public CharacterEntity owningCharacter;
    public int indexOfData;

    [Header("Generic Info Format")]
    [Tooltip("Title Format => {0} = {Title}")]
    public string titleFormat = "{0}";
    [Tooltip("Description Format => {0} = {Description}")]
    public string descriptionFormat = "{0}";
    [Tooltip("Level Format => {0} = {Level}")]
    public string levelFormat = "{0}";

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public Text textLevel;
    public Image imageIcon;
    public Button addButton;

    private void Update()
    {
        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, attribute == null ? "Unknow" : attribute.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, attribute == null ? "N/A" : attribute.description);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, data.ToString("N0"));

        if (imageIcon != null)
        {
            imageIcon.sprite = attribute == null ? null : attribute.icon;
            imageIcon.gameObject.SetActive(attribute != null);
        }
        
        if (addButton != null)
            addButton.interactable = owningCharacter != null && owningCharacter.StatPoint > 0;
    }

    public override void Show()
    {
        base.Show();
        if (addButton != null)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(OnClickAdd);
        }
    }

    private void OnClickAdd()
    {
        if (owningCharacter != null)
            owningCharacter.AddAttributeLevel(indexOfData);
    }
}
