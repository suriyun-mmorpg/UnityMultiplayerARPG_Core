using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterAttribute : UIDataForCharacter<KeyValuePair<CharacterAttribute, int>>
{
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

    [Header("Events")]
    public UnityEvent onAbleToIncrease;
    public UnityEvent onUnableToIncrease;
    
    private void Update()
    {
        var characterAttribute = Data.Key;
        var amount = Data.Value;
        
        if (IsOwningCharacter() && characterAttribute.CanIncrease(PlayerCharacterController.OwningCharacter))
            onAbleToIncrease.Invoke();
        else
            onUnableToIncrease.Invoke();
    }

    protected override void UpdateData()
    {
        var owningCharacter = PlayerCharacterController.OwningCharacter;
        var characterAttribute = Data.Key;
        var attribute = characterAttribute.GetAttribute();
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

    public void OnClickAdd()
    {
        if (!IsOwningCharacter())
            return;

        var owningCharacter = PlayerCharacterController.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.RequestAddAttribute(indexOfData, 1);
    }
}
