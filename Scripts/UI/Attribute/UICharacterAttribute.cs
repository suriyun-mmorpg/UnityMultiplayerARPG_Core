using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterAttribute : UIDataForCharacter<CharacterAttributeAmountTuple>
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
        public TextWrapper uiTextTitle;
        public Text textDescription;
        public TextWrapper uiTextDescription;
        public Text textAmount;
        public TextWrapper uiTextAmount;
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
            MigrateUIComponents();
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            var characterAttribute = Data.characterAttribute;
            var attribute = characterAttribute.GetAttribute();
            var amount = Data.targetAmount;

            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(titleFormat, attribute == null ? "Unknow" : attribute.title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, attribute == null ? "N/A" : attribute.description);

            if (uiTextAmount != null)
                uiTextAmount.text = string.Format(amountFormat, amount.ToString("N0"));

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

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextTitle = MigrateUIHelpers.SetWrapperToText(textTitle, uiTextTitle);
            uiTextDescription = MigrateUIHelpers.SetWrapperToText(textDescription, uiTextDescription);
            uiTextAmount = MigrateUIHelpers.SetWrapperToText(textAmount, uiTextAmount);
        }
    }
}
