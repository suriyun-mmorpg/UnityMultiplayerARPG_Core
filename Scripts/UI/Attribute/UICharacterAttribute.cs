using Cysharp.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterAttribute : UIDataForCharacter<UICharacterAttributeData>
    {
        public CharacterAttribute CharacterAttribute { get { return Data.characterAttribute; } }
        public float Amount { get { return Data.targetAmount; } }
        public Attribute Attribute { get { return CharacterAttribute != null ? CharacterAttribute.GetAttribute() : null; } }
        
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Description}")]
        public UILocaleKeySetting formatKeyDescription = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public TextWrapper uiTextAmount;
        public Image imageIcon;

        [Header("Events")]
        public UnityEvent onAbleToIncrease;
        public UnityEvent onUnableToIncrease;

        protected override void UpdateUI()
        {
            if (Character is IPlayerCharacterData playerCharacter && Attribute.CanIncreaseAmount(playerCharacter, CharacterAttribute.amount, out _))
                onAbleToIncrease.Invoke();
            else
                onUnableToIncrease.Invoke();
        }

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
            {
                uiTextTitle.text = ZString.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Attribute == null ? LanguageManager.GetUnknowTitle() : Attribute.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = ZString.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Attribute == null ? LanguageManager.GetUnknowDescription() : Attribute.Description);
            }

            if (uiTextAmount != null)
            {
                uiTextAmount.text = ZString.Format(
                    LanguageManager.GetText(formatKeyAmount),
                    Amount.ToString("N0"));
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = Attribute == null ? null : Attribute.Icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
                imageIcon.preserveAspect = true;
            }
        }

        public void OnClickAdd()
        {
            GameInstance.ClientCharacterHandlers.RequestIncreaseAttributeAmount(new RequestIncreaseAttributeAmountMessage()
            {
                dataId = Attribute.DataId
            }, ClientCharacterActions.ResponseIncreaseAttributeAmount);
        }
    }
}
