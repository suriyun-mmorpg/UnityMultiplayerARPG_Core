using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterAttribute : UIDataForCharacter<AttributeTuple>
    {
        public Attribute Attribute { get { return Data.attribute; } }
        public short Amount { get { return Data.targetAmount; } }

        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";
        [Tooltip("Amount Format => {0} = {Amount}")]
        public string amountFormat = "{0}";

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
            Profiler.BeginSample("UICharacterAttribute - Update UI");
            if (IsOwningCharacter() && BasePlayerCharacterController.OwningCharacter.StatPoint > 0)
                onAbleToIncrease.Invoke();
            else
                onUnableToIncrease.Invoke();
            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(titleFormat, Attribute == null ? "Unknow" : Attribute.title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, Attribute == null ? "N/A" : Attribute.description);

            if (uiTextAmount != null)
                uiTextAmount.text = string.Format(amountFormat, Amount.ToString("N0"));

            if (imageIcon != null)
            {
                var iconSprite = Attribute == null ? null : Attribute.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }
        }

        public void OnClickAdd()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
                return;

            owningCharacter.RequestAddAttribute(Attribute.DataId);
        }
    }
}
