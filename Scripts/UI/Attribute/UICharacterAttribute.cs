using Cysharp.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterAttribute : UIDataForCharacter<UICharacterAttributeData>
    {
        public CharacterAttribute CharacterAttribute { get { return Data.characterAttribute; } }
        public float Amount { get { return Data.targetAmount; } }
        public Attribute Attribute { get { return CharacterAttribute.GetAttribute(); } }

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

        [Header("Bonus Stats")]
        public UICharacterStats uiIncreaseStats;
        public UIResistanceAmounts uiIncreaseResistances;
        public UIArmorAmounts uiIncreaseArmors;
        public UIDamageElementAmounts uiIncreaseDamages;
        public UIStatusEffectResistances uiStatusEffectResistances;

        [Header("Events")]
        public UnityEvent onAbleToIncrease = new UnityEvent();
        public UnityEvent onUnableToIncrease = new UnityEvent();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            uiTextTitle = null;
            uiTextDescription = null;
            uiTextAmount = null;
            imageIcon = null;
            uiIncreaseStats = null;
            uiIncreaseResistances = null;
            uiIncreaseArmors = null;
            uiIncreaseDamages = null;
            uiStatusEffectResistances = null;
            onAbleToIncrease?.RemoveAllListeners();
            onUnableToIncrease?.RemoveAllListeners();
        }

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
            }

            if (uiIncreaseStats != null)
            {
                CharacterStats stats = new CharacterStats();
                if (Attribute != null)
                {
                    stats += Attribute.GetStats(Amount);
                }

                if (stats.IsEmpty())
                {
                    // Hide ui if stats is empty
                    uiIncreaseStats.Hide();
                }
                else
                {
                    uiIncreaseStats.displayType = UICharacterStats.DisplayType.Simple;
                    uiIncreaseStats.isBonus = true;
                    uiIncreaseStats.Show();
                    uiIncreaseStats.Data = stats;
                }
            }

            if (uiIncreaseResistances != null)
            {
                Dictionary<DamageElement, float> resistances = null;
                if (Attribute != null)
                {
                    resistances = Attribute.GetIncreaseResistances(Amount);
                }

                if (resistances == null || resistances.Count == 0)
                {
                    // Hide ui if resistances is empty
                    uiIncreaseResistances.Hide();
                }
                else
                {
                    uiIncreaseResistances.isBonus = true;
                    uiIncreaseResistances.Show();
                    uiIncreaseResistances.Data = resistances;
                }
            }

            if (uiIncreaseArmors != null)
            {
                Dictionary<DamageElement, float> armors = null;
                if (Attribute != null)
                {
                    armors = Attribute.GetIncreaseArmors(Amount);
                }

                if (armors == null || armors.Count == 0)
                {
                    // Hide ui if armors is empty
                    uiIncreaseArmors.Hide();
                }
                else
                {
                    uiIncreaseArmors.displayType = UIArmorAmounts.DisplayType.Simple;
                    uiIncreaseArmors.isBonus = true;
                    uiIncreaseArmors.Show();
                    uiIncreaseArmors.Data = armors;
                }
            }

            if (uiIncreaseDamages != null)
            {
                Dictionary<DamageElement, MinMaxFloat> damageAmounts = null;
                if (Attribute != null)
                {
                    damageAmounts = Attribute.GetIncreaseDamages(Amount);
                }

                if (damageAmounts == null || damageAmounts.Count == 0)
                {
                    // Hide ui if damage amounts is empty
                    uiIncreaseDamages.Hide();
                }
                else
                {
                    uiIncreaseDamages.displayType = UIDamageElementAmounts.DisplayType.Simple;
                    uiIncreaseDamages.isBonus = true;
                    uiIncreaseDamages.Show();
                    uiIncreaseDamages.Data = damageAmounts;
                }
            }

            if (uiStatusEffectResistances != null)
            {
                Dictionary<StatusEffect, float> statusEffectResistances = null;
                if (Attribute != null)
                {
                    statusEffectResistances = Attribute.GetIncreaseStatusEffectResistances(Amount);
                }

                if (statusEffectResistances == null || statusEffectResistances.Count == 0)
                {
                    // Hide ui if armors is empty
                    uiStatusEffectResistances.Hide();
                }
                else
                {
                    uiStatusEffectResistances.isBonus = true;
                    uiStatusEffectResistances.Show();
                    uiStatusEffectResistances.UpdateData(statusEffectResistances);
                }
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
