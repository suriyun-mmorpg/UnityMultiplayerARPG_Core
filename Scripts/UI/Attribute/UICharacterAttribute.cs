using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterAttribute : UISelectionEntry<KeyValuePair<CharacterAttribute, int>>
{
    public int indexOfData { get; protected set; }

    [Header("Generic Info Format")]
    [Tooltip("Title Format => {0} = {Title}")]
    public string titleFormat = "{0}";
    [Tooltip("Description Format => {0} = {Description}")]
    public string descriptionFormat = "{0}";
    [Tooltip("Amount Format => {0} = {Amount}")]
    public string amountFormat = "{0}";

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public Text textAmount;
    public Image imageIcon;
    public Button addButton;

    public void Setup(KeyValuePair<CharacterAttribute, int> data, int indexOfData)
    {
        this.indexOfData = indexOfData;
        Data = data;
    }

    private void Update()
    {
        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (addButton != null)
            addButton.interactable = Data.Key.CanIncrease(owningCharacter);
    }

    protected override void UpdateData()
    {
        var characterAttribute = Data.Key;
        var attribute = characterAttribute.GetAttribute();
        var amount = Data.Value;

        if (addButton != null)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(OnClickAdd);
        }

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, attribute == null ? "Unknow" : attribute.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, attribute == null ? "N/A" : attribute.description);

        if (textAmount != null)
            textAmount.text = string.Format(amountFormat, amount.ToString("N0"));

        if (imageIcon != null)
        {
            var iconSprite = attribute == null ? null : attribute.icon;
            imageIcon.sprite = iconSprite;
            imageIcon.gameObject.SetActive(iconSprite != null);
        }
    }

    private void OnClickAdd()
    {
        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.RequestAddAttribute(indexOfData);
    }
}
