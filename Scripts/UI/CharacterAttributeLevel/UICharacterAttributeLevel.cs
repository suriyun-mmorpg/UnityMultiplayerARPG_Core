using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterAttributeLevel : UISelectionEntry<CharacterAttributeLevel>
{
    public CharacterAttribute attribute;

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
    public UICharacterStats uiCharacterStats;

    private void Update()
    {
        var uiSceneGameplay = UISceneGameplay.Singleton;
        Dictionary<CharacterAttribute, int> attributes = null;
        int attributeLevel = 0;
        var character = CharacterEntity.OwningCharacter;
        if (character != null)
        {
            attributes = character.GetAttributes();
            attributeLevel = attributes[attribute];
        }
        else if (data != null)
        {
            attributeLevel = data.level;
        }

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, attribute == null ? "Unknow" : attribute.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, attribute == null ? "N/A" : attribute.description);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, data == null ? "N/A" : attributeLevel.ToString("N0"));

        if (imageIcon != null)
            imageIcon.sprite = attribute == null ? null : attribute.icon;

        var stats = CharacterDataHelpers.GetStatsByAttributeAmountPairs(attributes);
        if (uiCharacterStats != null)
            uiCharacterStats.data = stats;

        if (addButton != null)
            addButton.interactable = character != null && character.StatPoint > 0;
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
        var owningCharacter = CharacterEntity.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.AddAttributeLevel(owningCharacter.attributeLevels.IndexOf(data));
    }
}
