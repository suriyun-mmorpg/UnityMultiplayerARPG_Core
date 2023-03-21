using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIBuff : UISelectionEntry<UIBuffData>
    {
        public Buff Buff { get { return Data.buff; } }
        public int Level { get { return Data.targetLevel; } }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Buff Duration}")]
        public UILocaleKeySetting formatKeyDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BUFF_DURATION);
        [Tooltip("Format => {0} = {Buff Recovery Hp}")]
        public UILocaleKeySetting formatKeyRecoveryHp = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BUFF_RECOVERY_HP);
        [Tooltip("Format => {0} = {Buff Recovery Mp}")]
        public UILocaleKeySetting formatKeyRecoveryMp = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BUFF_RECOVERY_MP);
        [Tooltip("Format => {0} = {Buff Recovery Stamina}")]
        public UILocaleKeySetting formatKeyRecoveryStamina = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BUFF_RECOVERY_STAMINA);
        [Tooltip("Format => {0} = {Buff Recovery Food}")]
        public UILocaleKeySetting formatKeyRecoveryFood = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BUFF_RECOVERY_FOOD);
        [Tooltip("Format => {0} = {Buff Recovery Water}")]
        public UILocaleKeySetting formatKeyRecoveryWater = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BUFF_RECOVERY_WATER);

        [Header("UI Elements")]
        public TextWrapper uiTextDuration;
        public TextWrapper uiTextRecoveryHp;
        public TextWrapper uiTextRecoveryMp;
        public TextWrapper uiTextRecoveryStamina;
        public TextWrapper uiTextRecoveryFood;
        public TextWrapper uiTextRecoveryWater;
        public UICharacterStats uiBuffStats;
        public UICharacterStats uiBuffStatsRate;
        public UIAttributeAmounts uiBuffAttributes;
        public UIAttributeAmounts uiBuffAttributesRate;
        public UIResistanceAmounts uiBuffResistances;
        public UIArmorAmounts uiBuffArmors;
        public UIDamageElementAmounts uiBuffDamages;
        public UIDamageElementAmounts uiDamageOverTimes;
        [Tooltip("This will activate if buff's disallow move is `TRUE`, developer may set text or icon here")]
        public GameObject disallowMoveObject;
        [Tooltip("This will activate if buff's disallow sprint is `TRUE`, developer may set text or icon here")]
        public GameObject disallowSprintObject;
        [Tooltip("This will activate if buff's disallow walk is `TRUE`, developer may set text or icon here")]
        public GameObject disallowWalkObject;
        [Tooltip("This will activate if buff's disallow jump is `TRUE`, developer may set text or icon here")]
        public GameObject disallowJumpObject;
        [Tooltip("This will activate if buff's disallow crouch is `TRUE`, developer may set text or icon here")]
        public GameObject disallowCrouchObject;
        [Tooltip("This will activate if buff's disallow prone is `TRUE`, developer may set text or icon here")]
        public GameObject disallowCrawlObject;
        [Tooltip("This will activate if buff's disallow attack is `TRUE`, developer may set text or icon here")]
        public GameObject disallowAttackObject;
        [Tooltip("This will activate if buff's disallow use skill is `TRUE`, developer may set text or icon here")]
        public GameObject disallowUseSkillObject;
        [Tooltip("This will activate if buff's disallow use item is `TRUE`, developer may set text or icon here")]
        public GameObject disallowUseItemObject;

        protected override void UpdateData()
        {
            if (uiTextDuration != null)
            {
                float duration = Buff.GetDuration(Level);
                uiTextDuration.SetGameObjectActive(duration != 0);
                uiTextDuration.text = ZString.Format(
                    LanguageManager.GetText(formatKeyDuration),
                    duration.ToString("N0"));
            }

            if (uiTextRecoveryHp != null)
            {
                int recoveryHp = Buff.GetRecoveryHp(Level);
                uiTextRecoveryHp.SetGameObjectActive(recoveryHp != 0);
                uiTextRecoveryHp.text = ZString.Format(
                    LanguageManager.GetText(formatKeyRecoveryHp),
                    recoveryHp.ToString("N0"));
            }

            if (uiTextRecoveryMp != null)
            {
                int recoveryMp = Buff.GetRecoveryMp(Level);
                uiTextRecoveryMp.SetGameObjectActive(recoveryMp != 0);
                uiTextRecoveryMp.text = ZString.Format(
                    LanguageManager.GetText(formatKeyRecoveryMp),
                    recoveryMp.ToString("N0"));
            }

            if (uiTextRecoveryStamina != null)
            {
                int recoveryStamina = Buff.GetRecoveryStamina(Level);
                uiTextRecoveryStamina.SetGameObjectActive(recoveryStamina != 0);
                uiTextRecoveryStamina.text = ZString.Format(
                    LanguageManager.GetText(formatKeyRecoveryStamina),
                    recoveryStamina.ToString("N0"));
            }

            if (uiTextRecoveryFood != null)
            {
                int recoveryFood = Buff.GetRecoveryFood(Level);
                uiTextRecoveryFood.SetGameObjectActive(recoveryFood != 0);
                uiTextRecoveryFood.text = ZString.Format(
                    LanguageManager.GetText(formatKeyRecoveryFood),
                    recoveryFood.ToString("N0"));
            }

            if (uiTextRecoveryWater != null)
            {
                int recoveryWater = Buff.GetRecoveryWater(Level);
                uiTextRecoveryWater.SetGameObjectActive(recoveryWater != 0);
                uiTextRecoveryWater.text = ZString.Format(
                    LanguageManager.GetText(formatKeyRecoveryWater),
                    recoveryWater.ToString("N0"));
            }

            if (uiBuffStats != null)
            {
                CharacterStats stats = Buff.GetIncreaseStats(Level);
                if (stats.IsEmpty())
                {
                    uiBuffStats.Hide();
                }
                else
                {
                    uiBuffStats.displayType = UICharacterStats.DisplayType.Simple;
                    uiBuffStats.isBonus = true;
                    uiBuffStats.Show();
                    uiBuffStats.Data = stats;
                }
            }

            if (uiBuffStatsRate != null)
            {
                CharacterStats statsRate = Buff.GetIncreaseStatsRate(Level);
                if (statsRate.IsEmpty())
                {
                    uiBuffStatsRate.Hide();
                }
                else
                {
                    uiBuffStatsRate.displayType = UICharacterStats.DisplayType.Rate;
                    uiBuffStatsRate.isBonus = true;
                    uiBuffStatsRate.Show();
                    uiBuffStatsRate.Data = statsRate;
                }
            }

            if (uiBuffAttributes != null)
            {
                if (Buff.increaseAttributes == null || Buff.increaseAttributes.Length == 0)
                {
                    uiBuffAttributes.Hide();
                }
                else
                {
                    uiBuffAttributes.displayType = UIAttributeAmounts.DisplayType.Simple;
                    uiBuffAttributes.isBonus = true;
                    uiBuffAttributes.Show();
                    uiBuffAttributes.Data = GameDataHelpers.CombineAttributes(Buff.increaseAttributes, new Dictionary<Attribute, float>(), Level, 1f);
                }
            }

            if (uiBuffAttributesRate != null)
            {
                if (Buff.increaseAttributesRate == null || Buff.increaseAttributesRate.Length == 0)
                {
                    uiBuffAttributesRate.Hide();
                }
                else
                {
                    uiBuffAttributesRate.displayType = UIAttributeAmounts.DisplayType.Rate;
                    uiBuffAttributesRate.isBonus = true;
                    uiBuffAttributesRate.Show();
                    uiBuffAttributesRate.Data = GameDataHelpers.CombineAttributes(Buff.increaseAttributesRate, new Dictionary<Attribute, float>(), Level, 1f);
                }
            }

            if (uiBuffResistances != null)
            {
                if (Buff.increaseResistances == null || Buff.increaseResistances.Length == 0)
                {
                    uiBuffResistances.Hide();
                }
                else
                {
                    uiBuffResistances.isBonus = true;
                    uiBuffResistances.Show();
                    uiBuffResistances.Data = GameDataHelpers.CombineResistances(Buff.increaseResistances, new Dictionary<DamageElement, float>(), Level, 1f);
                }
            }

            if (uiBuffArmors != null)
            {
                if (Buff.increaseArmors == null || Buff.increaseArmors.Length == 0)
                {
                    uiBuffArmors.Hide();
                }
                else
                {
                    uiBuffArmors.isBonus = true;
                    uiBuffArmors.Show();
                    uiBuffArmors.Data = GameDataHelpers.CombineArmors(Buff.increaseArmors, new Dictionary<DamageElement, float>(), Level, 1f);
                }
            }

            if (uiBuffDamages != null)
            {
                if (Buff.increaseDamages == null || Buff.increaseDamages.Length == 0)
                {
                    uiBuffDamages.Hide();
                }
                else
                {
                    uiBuffDamages.isBonus = true;
                    uiBuffDamages.Show();
                    uiBuffDamages.Data = GameDataHelpers.CombineDamages(Buff.increaseDamages, new Dictionary<DamageElement, MinMaxFloat>(), Level, 1f);
                }
            }

            if (uiDamageOverTimes != null)
            {
                if (Buff.damageOverTimes == null || Buff.damageOverTimes.Length == 0)
                {
                    uiDamageOverTimes.Hide();
                }
                else
                {
                    uiDamageOverTimes.isBonus = false;
                    uiDamageOverTimes.Show();
                    uiDamageOverTimes.Data = GameDataHelpers.CombineDamages(Buff.damageOverTimes, new Dictionary<DamageElement, MinMaxFloat>(), Level, 1f);
                }
            }

            if (disallowMoveObject != null)
                disallowMoveObject.SetActive(Data.buff.disallowMove);

            if (disallowSprintObject != null)
                disallowSprintObject.SetActive(Data.buff.disallowSprint);

            if (disallowWalkObject != null)
                disallowWalkObject.SetActive(Data.buff.disallowWalk);

            if (disallowJumpObject != null)
                disallowJumpObject.SetActive(Data.buff.disallowJump);

            if (disallowCrouchObject != null)
                disallowCrouchObject.SetActive(Data.buff.disallowCrouch);

            if (disallowCrawlObject != null)
                disallowCrawlObject.SetActive(Data.buff.disallowCrawl);

            if (disallowAttackObject != null)
                disallowAttackObject.SetActive(Data.buff.disallowAttack);

            if (disallowUseSkillObject != null)
                disallowUseSkillObject.SetActive(Data.buff.disallowUseSkill);

            if (disallowUseItemObject != null)
                disallowUseItemObject.SetActive(Data.buff.disallowUseItem);
        }
    }
}
