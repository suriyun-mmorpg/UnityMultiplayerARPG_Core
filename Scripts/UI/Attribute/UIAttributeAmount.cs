using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAttributeAmount : UISelectionEntry<KeyValuePair<Attribute, int>>
{
    [System.NonSerialized]
    public int indexOfData;

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

    private void Update()
    {
        var owningCharacter = CharacterEntity.OwningCharacter;
        if (addButton != null)
            addButton.interactable = owningCharacter != null && owningCharacter.StatPoint > 0;
    }

    protected override void UpdateData()
    {
        var attribute = Data.Key;
        var amount = Data.Value;

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
            owningCharacter.AddAttribute(indexOfData);
    }
}
