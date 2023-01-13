using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterSkill : UIDataForCharacter<UICharacterSkillData>
    {
        public CharacterSkill CharacterSkill { get { return Data.characterSkill; } }
        public int Level { get { return Data.targetLevel; } }
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
        [Tooltip("Format => {0} = {List Of Armor Type}")]
        public UILocaleKeySetting formatKeyAvailableArmors = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_AVAILABLE_WEAPONS);
        [Tooltip("Format => {0} = {List Of Vehicle Type}")]
        public UILocaleKeySetting formatKeyAvailableVehicles = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_AVAILABLE_WEAPONS);
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
        public TextWrapper uiTextAvailableArmors;
        public TextWrapper uiTextAvailableVehicles;
        public TextWrapper uiTextConsumeHp;
        public TextWrapper uiTextConsumeMp;
        public TextWrapper uiTextConsumeStamina;
        public TextWrapper uiTextCoolDownDuration;
        public TextWrapper uiTextCoolDownRemainsDuration;
        public Image imageCoolDownGage;
        public GameObject[] countDownObjects;
        public GameObject[] noCountDownObjects;
        public UISkillRequirement uiRequirement;
        public TextWrapper uiTextSummon;
        public TextWrapper uiTextMount;
        public UIItemCraft uiCraftItem;

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
        protected bool dirtyIsCountDown;

        protected override void OnDisable()
        {
            base.OnDisable();
            coolDownRemainsDuration = 0f;
        }

        protected override void Update()
        {
            base.Update();

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
                uiTextCoolDownDuration.SetGameObjectActive(Skill.IsActive && coolDownDuration > 0f);
                uiTextCoolDownDuration.text = ZString.Format(
                    LanguageManager.GetText(formatKeyCoolDownDuration),
                    coolDownDuration.ToString("N0"));
            }

            if (uiTextCoolDownRemainsDuration != null)
            {
                uiTextCoolDownRemainsDuration.SetGameObjectActive(Skill.IsActive && coolDownRemainsDuration > 0);
                uiTextCoolDownRemainsDuration.text = ZString.Format(
                    LanguageManager.GetText(formatKeyCoolDownRemainsDuration),
                    coolDownRemainsDuration.ToString("N0"));
            }

            if (imageCoolDownGage != null)
            {
                imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 0 : coolDownRemainsDuration / coolDownDuration;
                imageCoolDownGage.gameObject.SetActive(imageCoolDownGage.fillAmount > 0f);
            }

            bool isCountDown = coolDownRemainsDuration > 0f;
            if (dirtyIsCountDown != isCountDown)
            {
                dirtyIsCountDown = isCountDown;
                if (countDownObjects != null)
                {
                    foreach (GameObject obj in countDownObjects)
                    {
                        obj.SetActive(isCountDown);
                    }
                }
                if (noCountDownObjects != null)
                {
                    foreach (GameObject obj in noCountDownObjects)
                    {
                        obj.SetActive(!isCountDown);
                    }
                }
            }
        }

        protected override void UpdateUI()
        {
            // Update remains duration
            if (coolDownRemainsDuration <= 0f && Character != null && Skill != null)
            {
                int indexOfSkillUsage = Character.IndexOfSkillUsage(Skill.DataId, SkillUsageType.Skill);
                if (indexOfSkillUsage >= 0)
                    coolDownRemainsDuration = Character.SkillUsages[indexOfSkillUsage].coolDownRemainsDuration;
            }

            IPlayerCharacterData targetPlayer = Character as IPlayerCharacterData;
            if (targetPlayer == null)
                targetPlayer = GameInstance.PlayingCharacter;
            if (targetPlayer != null && Skill != null && Skill.CanLevelUp(targetPlayer, CharacterSkill.level, out _))
                onAbleToLevelUp.Invoke();
            else
                onUnableToLevelUp.Invoke();
        }

        protected override void UpdateData()
        {
            // Update remains duration
            if (Character != null && Skill != null)
            {
                int indexOfSkillUsage = Character.IndexOfSkillUsage(Skill.DataId, SkillUsageType.Skill);
                if (indexOfSkillUsage >= 0 && Mathf.Abs(Character.SkillUsages[indexOfSkillUsage].coolDownRemainsDuration - coolDownRemainsDuration) > 1)
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
                uiTextTitle.text = ZString.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Skill == null ? LanguageManager.GetUnknowTitle() : Skill.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = ZString.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Skill == null ? LanguageManager.GetUnknowDescription() : Skill.Description);
            }

            if (uiTextLevel != null)
            {
                uiTextLevel.text = ZString.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Level.ToString("N0"));
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = Skill == null ? null : Skill.Icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
                imageIcon.preserveAspect = true;
            }

            if (uiTextSkillType != null)
            {
                uiTextSkillType.text = ZString.Format(
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
                    uiTextAvailableWeapons.text = ZString.Format(
                        LanguageManager.GetText(formatKeyAvailableWeapons),
                        Skill.AvailableWeaponsText);
                }
            }

            if (uiTextAvailableArmors != null)
            {
                if (string.IsNullOrEmpty(Skill.AvailableArmorsText))
                {
                    uiTextAvailableArmors.SetGameObjectActive(false);
                }
                else
                {
                    uiTextAvailableArmors.SetGameObjectActive(true);
                    uiTextAvailableArmors.text = ZString.Format(
                        LanguageManager.GetText(formatKeyAvailableArmors),
                        Skill.AvailableArmorsText);
                }
            }

            if (uiTextAvailableVehicles != null)
            {
                if (string.IsNullOrEmpty(Skill.AvailableVehiclesText))
                {
                    uiTextAvailableVehicles.SetGameObjectActive(false);
                }
                else
                {
                    uiTextAvailableVehicles.SetGameObjectActive(true);
                    uiTextAvailableVehicles.text = ZString.Format(
                        LanguageManager.GetText(formatKeyAvailableVehicles),
                        Skill.AvailableVehiclesText);
                }
            }

            int tempConsumeAmount;

            if (uiTextConsumeHp != null)
            {
                tempConsumeAmount = Skill == null || Level <= 0 ? 0 : Skill.GetTotalConsumeHp(Level, Character);
                if (tempConsumeAmount != 0)
                {
                    uiTextConsumeHp.text = ZString.Format(
                        LanguageManager.GetText(formatKeyConsumeHp),
                        tempConsumeAmount.ToString("N0"));
                    uiTextConsumeHp.SetGameObjectActive(true);
                }
                else
                {
                    uiTextConsumeHp.SetGameObjectActive(false);
                }
            }

            if (uiTextConsumeMp != null)
            {
                tempConsumeAmount = Skill == null || Level <= 0 ? 0 : Skill.GetTotalConsumeMp(Level, Character);
                if (tempConsumeAmount != 0)
                {
                    uiTextConsumeMp.text = ZString.Format(
                        LanguageManager.GetText(formatKeyConsumeMp),
                        tempConsumeAmount.ToString("N0"));
                    uiTextConsumeMp.SetGameObjectActive(true);
                }
                else
                {
                    uiTextConsumeMp.SetGameObjectActive(false);
                }
            }

            if (uiTextConsumeStamina != null)
            {
                tempConsumeAmount = Skill == null || Level <= 0 ? 0 : Skill.GetTotalConsumeStamina(Level, Character);
                if (tempConsumeAmount != 0)
                {
                    uiTextConsumeStamina.text = ZString.Format(
                        LanguageManager.GetText(formatKeyConsumeStamina),
                        tempConsumeAmount.ToString("N0"));
                    uiTextConsumeStamina.SetGameObjectActive(true);
                }
                else
                {
                    uiTextConsumeStamina.SetGameObjectActive(false);
                }
            }

            if (uiRequirement != null)
            {
                if (Skill == null || Level <= 0 ||
                    (!Skill.IsDisallowToLevelUp(Level) &&
                    Skill.GetRequireCharacterLevel(Level) <= 0 &&
                    Skill.GetRequireCharacterSkillPoint(Level) <= 0 &&
                    Skill.GetRequireCharacterGold(Level) <= 0 &&
                    Skill.GetRequireAttributeAmounts(Level).Count == 0) &&
                    Skill.GetRequireSkillLevels(Level).Count == 0 &&
                    Skill.GetRequireCurrencyAmounts(Level).Count == 0 &&
                    Skill.GetRequireItemAmounts(Level).Count == 0)
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
                if (Skill == null || !Skill.IsActive || Skill.Summon.MonsterEntity == null)
                {
                    uiTextSummon.SetGameObjectActive(false);
                }
                else
                {
                    uiTextSummon.SetGameObjectActive(true);
                    uiTextSummon.text = ZString.Format(
                        LanguageManager.GetText(formatKeySummon),
                        Skill.Summon.MonsterEntity.Title,
                        Skill.Summon.Level.GetAmount(Level),
                        Skill.Summon.AmountEachTime.GetAmount(Level),
                        Skill.Summon.MaxStack.GetAmount(Level),
                        Skill.Summon.Duration.GetAmount(Level));
                }
            }

            if (uiTextMount != null)
            {
                if (Skill == null || !Skill.IsActive || Skill.Mount.MountEntity == null)
                {
                    uiTextMount.SetGameObjectActive(false);
                }
                else
                {
                    uiTextMount.SetGameObjectActive(true);
                    uiTextMount.text = ZString.Format(
                        LanguageManager.GetText(formatKeyMount),
                        Skill.Mount.MountEntity.Title);
                }
            }

            if (uiCraftItem != null)
            {
                if (Skill == null || !Skill.IsCraftItem)
                {
                    uiCraftItem.Hide();
                }
                else
                {
                    uiCraftItem.Setup(CrafterType.Character, null, Skill.ItemCraft);
                    uiCraftItem.Show();
                }
            }

            bool isAttack = Skill != null && Skill.IsAttack;
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
                if (!Skill.IsBuff)
                {
                    uiSkillBuff.Hide();
                }
                else
                {
                    uiSkillBuff.Show();
                    uiSkillBuff.Data = new UIBuffData(Skill.Buff, Level);
                }
            }

            if (uiSkillDebuff != null)
            {
                if (!Skill.IsDebuff)
                {
                    uiSkillDebuff.Hide();
                }
                else
                {
                    uiSkillDebuff.Show();
                    uiSkillDebuff.Data = new UIBuffData(Skill.Debuff, Level);
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
                    uiNextLevelSkill.Setup(new UICharacterSkillData(CharacterSkill, (Level + 1)), Character, IndexOfData);
                    uiNextLevelSkill.Show();
                }
            }
        }

        public void OnClickAdd()
        {
            GameInstance.ClientCharacterHandlers.RequestIncreaseSkillLevel(new RequestIncreaseSkillLevelMessage()
            {
                dataId = Skill.DataId
            }, ClientCharacterActions.ResponseIncreaseSkillLevel);
        }
    }
}
