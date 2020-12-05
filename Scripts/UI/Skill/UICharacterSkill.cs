using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterSkill : UIDataForCharacter<UICharacterSkillData>
    {
        public CharacterSkill CharacterSkill { get { return Data.characterSkill; } }
        public short Level { get { return Data.targetLevel; } }
        public BaseSkill Skill { get { return CharacterSkill != null ? CharacterSkill.GetSkill() : null; } }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Description}")]
        public UILocaleKeySetting formatKeyDescription = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {List Of Weapon Type}")]
        public UILocaleKeySetting formatKeyAvailableWeapons = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_AVAILABLE_WEAPONS);
        [Tooltip("Format => {0} = {Consume Hp Amount}")]
        public UILocaleKeySetting formatKeyConsumeHp = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CONSUME_HP);
        [Tooltip("Format => {0} = {Consume Mp Amount}")]
        public UILocaleKeySetting formatKeyConsumeMp = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CONSUME_MP);
        [Tooltip("Format => {0} = {Consume Stamina Amount}")]
        public UILocaleKeySetting formatKeyConsumeStamina = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CONSUME_STAMINA);
        [Tooltip("Format => {0} = {Cooldown Duration}")]
        public UILocaleKeySetting formatKeyCoolDownDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_COOLDOWN_DURATION);
        [Tooltip("Format => {0} = {Cooldown Remains Duration}")]
        public UILocaleKeySetting formatKeyCoolDownRemainsDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Monster Title}, {1} = {Monster Level}, {2} = {Amount}, {3} = {Duration}")]
        public UILocaleKeySetting formatKeySummon = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_SUMMON);
        [Tooltip("Format => {0} = {Mount Title}")]
        public UILocaleKeySetting formatKeyMount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_MOUNT);
        [Tooltip("Format => {0} = {Skill Type Title}")]
        public UILocaleKeySetting formatKeySkillType = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_TYPE);

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public TextWrapper uiTextLevel;
        public Image imageIcon;
        public TextWrapper uiTextSkillType;
        public TextWrapper uiTextAvailableWeapons;
        public TextWrapper uiTextConsumeHp;
        public TextWrapper uiTextConsumeMp;
        public TextWrapper uiTextConsumeStamina;
        public TextWrapper uiTextCoolDownDuration;
        public TextWrapper uiTextCoolDownRemainsDuration;
        public Image imageCoolDownGage;
        public UISkillRequirement uiRequirement;
        public TextWrapper uiTextSummon;
        public TextWrapper uiTextMount;
        public UICraftItem uiCraftItem;

        [Header("Skill Attack")]
        public UIDamageElementAmount uiDamageAmount;
        public UIDamageElementInflictions uiDamageInflictions;
        public UIDamageElementAmounts uiAdditionalDamageAmounts;

        [Header("Buff/Debuff")]
        public UIBuff uiSkillBuff;
        public UIBuff uiSkillDebuff;

        [Header("Events")]
        public UnityEvent onSetLevelZeroData;
        public UnityEvent onSetNonLevelZeroData;
        public UnityEvent onAbleToLevelUp;
        public UnityEvent onUnableToLevelUp;

        [Header("Options")]
        [Tooltip("UIs set here will be cloned by this UI")]
        public UICharacterSkill[] clones;
        public UICharacterSkill uiNextLevelSkill;

        protected float coolDownRemainsDuration;

        protected override void OnDisable()
        {
            base.OnDisable();
            coolDownRemainsDuration = 0f;
        }

        protected override void Update()
        {
            base.Update();

            if (coolDownRemainsDuration <= 0f)
            {
                if (Character != null && Skill != null)
                {
                    int indexOfSkillUsage = Character.IndexOfSkillUsage(Skill.DataId, SkillUsageType.Skill);
                    if (indexOfSkillUsage >= 0)
                    {
                        coolDownRemainsDuration = Character.SkillUsages[indexOfSkillUsage].coolDownRemainsDuration;
                        if (coolDownRemainsDuration <= 1f)
                            coolDownRemainsDuration = 0f;
                    }
                }
            }

            if (coolDownRemainsDuration > 0f)
            {
                coolDownRemainsDuration -= Time.deltaTime;
                if (coolDownRemainsDuration <= 0f)
                    coolDownRemainsDuration = 0f;
            }
            else
            {
                coolDownRemainsDuration = 0f;
            }

            // Update UIs
            float coolDownDuration = Skill.GetCoolDownDuration(Level);

            if (uiTextCoolDownDuration != null)
            {
                uiTextCoolDownDuration.text = string.Format(
                    LanguageManager.GetText(formatKeyCoolDownDuration),
                    coolDownDuration.ToString("N0"));
            }

            if (uiTextCoolDownRemainsDuration != null)
            {
                uiTextCoolDownRemainsDuration.SetGameObjectActive(coolDownRemainsDuration > 0);
                uiTextCoolDownRemainsDuration.text = string.Format(
                    LanguageManager.GetText(formatKeyCoolDownRemainsDuration),
                    coolDownRemainsDuration.ToString("N0"));
            }

            if (imageCoolDownGage != null)
            {
                imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 0 : coolDownRemainsDuration / coolDownDuration;
            }
        }

        protected override void UpdateUI()
        {
            GameMessage.Type gameMessageType;
            if (IsOwningCharacter() && Skill.CanLevelUp(OwningCharacter, CharacterSkill.level, out gameMessageType))
            {
                onAbleToLevelUp.Invoke();
            }
            else
            {
                onUnableToLevelUp.Invoke();
            }
        }

        protected override void UpdateData()
        {
            // Update remains duration
            if (Character != null && Skill != null)
            {
                int indexOfSkillUsage = Character.IndexOfSkillUsage(Skill.DataId, SkillUsageType.Skill);
                if (indexOfSkillUsage >= 0)
                    coolDownRemainsDuration = Character.SkillUsages[indexOfSkillUsage].coolDownRemainsDuration;
            }

            if (Level <= 0)
            {
                onSetLevelZeroData.Invoke();
            }
            else
            {
                onSetNonLevelZeroData.Invoke();
            }

            if (uiTextTitle != null)
            {
                uiTextTitle.text = string.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Skill == null ? LanguageManager.GetUnknowTitle() : Skill.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = string.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Skill == null ? LanguageManager.GetUnknowDescription() : Skill.Description);
            }

            if (uiTextLevel != null)
            {
                uiTextLevel.text = string.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Level.ToString("N0"));
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = Skill == null ? null : Skill.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiTextSkillType != null)
            {
                uiTextSkillType.text = string.Format(
                    LanguageManager.GetText(formatKeySkillType),
                    Skill == null ? LanguageManager.GetUnknowTitle() : Skill.TypeTitle);
            }

            if (uiTextAvailableWeapons != null)
            {
                if (string.IsNullOrEmpty(Skill.AvailableWeaponsText))
                {
                    uiTextAvailableWeapons.SetGameObjectActive(false);
                }
                else
                {
                    uiTextAvailableWeapons.SetGameObjectActive(true);
                    uiTextAvailableWeapons.text = string.Format(
                        LanguageManager.GetText(formatKeyAvailableWeapons),
                        Skill.AvailableWeaponsText);
                }
            }

            if (uiTextConsumeHp != null)
            {
                uiTextConsumeHp.text = string.Format(
                    LanguageManager.GetText(formatKeyConsumeHp),
                    (Skill == null || Level <= 0) ?
                        LanguageManager.GetUnknowDescription() :
                        Skill.GetConsumeHp(Level).ToString("N0"));
            }

            if (uiTextConsumeMp != null)
            {
                uiTextConsumeMp.text = string.Format(
                    LanguageManager.GetText(formatKeyConsumeMp),
                    (Skill == null || Level <= 0) ?
                        LanguageManager.GetUnknowDescription() :
                        Skill.GetConsumeMp(Level).ToString("N0"));
            }

            if (uiTextConsumeStamina != null)
            {
                uiTextConsumeStamina.text = string.Format(
                    LanguageManager.GetText(formatKeyConsumeStamina),
                    (Skill == null || Level <= 0) ?
                        LanguageManager.GetUnknowDescription() :
                        Skill.GetConsumeStamina(Level).ToString("N0"));
            }

            if (uiRequirement != null)
            {
                if (Skill == null || (Skill.GetRequireCharacterLevel(Level) == 0 && Skill.CacheRequireSkillLevels.Count == 0))
                {
                    uiRequirement.Hide();
                }
                else
                {
                    uiRequirement.Show();
                    uiRequirement.Data = Data;
                }
            }

            if (uiTextSummon != null)
            {
                if (Skill == null || !Skill.IsActive() || Skill.GetSummon().MonsterEntity == null)
                {
                    uiTextSummon.SetGameObjectActive(false);
                }
                else
                {
                    uiTextSummon.SetGameObjectActive(true);
                    uiTextSummon.text = string.Format(
                        LanguageManager.GetText(formatKeySummon),
                        Skill.GetSummon().MonsterEntity.Title,
                        Skill.GetSummon().Level.GetAmount(Level),
                        Skill.GetSummon().AmountEachTime.GetAmount(Level),
                        Skill.GetSummon().MaxStack.GetAmount(Level),
                        Skill.GetSummon().Duration.GetAmount(Level));
                }
            }

            if (uiTextMount != null)
            {
                if (Skill == null || !Skill.IsActive() || Skill.GetMount().MountEntity == null)
                {
                    uiTextMount.SetGameObjectActive(false);
                }
                else
                {
                    uiTextMount.SetGameObjectActive(true);
                    uiTextMount.text = string.Format(
                        LanguageManager.GetText(formatKeyMount),
                        Skill.GetMount().MountEntity.Title);
                }
            }

            if (uiCraftItem != null)
            {
                if (Skill == null || !Skill.IsCraftItem())
                {
                    uiCraftItem.Hide();
                }
                else
                {
                    uiCraftItem.Setup(CrafterType.Character, null, Skill.GetItemCraft());
                    uiCraftItem.Show();
                }
            }

            bool isAttack = Skill != null && Skill.IsAttack();
            if (uiDamageAmount != null)
            {
                KeyValuePair<DamageElement, MinMaxFloat> baseAttackDamageAmount = Skill.GetBaseAttackDamageAmount(Character, Level, false);
                if (!isAttack)
                {
                    uiDamageAmount.Hide();
                }
                else
                {
                    uiDamageAmount.Show();
                    uiDamageAmount.Data = new UIDamageElementAmountData(baseAttackDamageAmount.Key, baseAttackDamageAmount.Value);
                }
            }

            if (uiDamageInflictions != null)
            {
                Dictionary<DamageElement, float> damageInflictionRates = Skill.GetAttackWeaponDamageInflictions(Character, Level);
                if (!isAttack || damageInflictionRates == null || damageInflictionRates.Count == 0)
                {
                    uiDamageInflictions.Hide();
                }
                else
                {
                    uiDamageInflictions.Show();
                    uiDamageInflictions.Data = damageInflictionRates;
                }
            }

            if (uiAdditionalDamageAmounts != null)
            {
                Dictionary<DamageElement, MinMaxFloat> additionalDamageAmounts = Skill.GetAttackAdditionalDamageAmounts(Character, Level);
                if (!isAttack || additionalDamageAmounts == null || additionalDamageAmounts.Count == 0)
                {
                    uiAdditionalDamageAmounts.Hide();
                }
                else
                {
                    uiAdditionalDamageAmounts.isBonus = false;
                    uiAdditionalDamageAmounts.Show();
                    uiAdditionalDamageAmounts.Data = additionalDamageAmounts;
                }
            }

            if (uiSkillBuff != null)
            {
                if (!Skill.IsBuff())
                {
                    uiSkillBuff.Hide();
                }
                else
                {
                    uiSkillBuff.Show();
                    uiSkillBuff.Data = new UIBuffData(Skill.GetBuff(), Level);
                }
            }

            if (uiSkillDebuff != null)
            {
                if (!Skill.IsDebuff())
                {
                    uiSkillDebuff.Hide();
                }
                else
                {
                    uiSkillDebuff.Show();
                    uiSkillDebuff.Data = new UIBuffData(Skill.GetDebuff(), Level);
                }
            }

            if (clones != null && clones.Length > 0)
            {
                for (int i = 0; i < clones.Length; ++i)
                {
                    if (clones[i] == null) continue;
                    clones[i].Data = Data;
                }
            }

            if (uiNextLevelSkill != null)
            {
                if (Level + 1 > Skill.maxLevel)
                {
                    uiNextLevelSkill.Hide();
                }
                else
                {
                    uiNextLevelSkill.Setup(new UICharacterSkillData(CharacterSkill, (short)(Level + 1)), Character, IndexOfData);
                    uiNextLevelSkill.Show();
                }
            }
        }

        public void OnClickAdd()
        {
            OwningCharacter.CallServerAddSkill(Skill.DataId);
        }
    }
}
