using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UICharacterAttribute : UIDataForCharacter<CharacterAttributeAmountTuple>
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

        protected override void UpdateUI()
        {
            var characterAttribute = Data.characterAttribute;

            if (IsOwningCharacter() && characterAttribute.CanIncrease(BasePlayerCharacterController.OwningCharacter))
                onAbleToIncrease.Invoke();
            else
                onUnableToIncrease.Invoke();
        }

        protected override void UpdateData()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            var characterAttribute = Data.characterAttribute;
            var attribute = characterAttribute.GetAttribute();
            var amount = Data.targetAmount;

            if (textTitle != null)
                textTitle.text = string.Format(titleFormat, attribute == null ? "Unknow" : attribute.title);

            if (textDescription != null)
                textDescription.text = string.Format(descriptionFormat, attribute == null ? "N/A" : attribute.description);

            if (textAmount != null)
                textAmount.text = string.Format(amountFormat, amount.ToString("N0"));

            if (imageIcon != null)
            {
                var iconSprite = attribute == null ? null : attribute.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }
        }

        public void OnClickAdd()
        {
            if (!IsOwningCharacter())
                return;

            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestAddAttribute(indexOfData, 1);
        }
    }
}
