using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterAttribute : UIDataForCharacter<CharacterAttributeTuple>
    {
        public CharacterAttribute CharacterAttribute { get { return Data.characterAttribute; } }
        public short Amount { get { return Data.targetAmount; } }
        public Attribute Attribute { get { return CharacterAttribute != null ? CharacterAttribute.GetAttribute() : null; } }
        
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Description}")]
        public UILocaleKeySetting formatKeyDescription = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_SIMPLE);

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
            if (IsOwningCharacter() && OwningCharacter.StatPoint > 0)
                onAbleToIncrease.Invoke();
            else
                onUnableToIncrease.Invoke();
        }

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
            {
                uiTextTitle.text = string.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Attribute == null ? LanguageManager.GetUnknowTitle() : Attribute.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = string.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Attribute == null ? LanguageManager.GetUnknowDescription() : Attribute.Description);
            }

            if (uiTextAmount != null)
            {
                uiTextAmount.text = string.Format(
                    LanguageManager.GetText(formatKeyAmount),
                    Amount.ToString("N0"));
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = Attribute == null ? null : Attribute.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }
        }

        public void OnClickAdd()
        {
            OwningCharacter.RequestAddAttribute(Attribute.DataId);
        }
    }
}
