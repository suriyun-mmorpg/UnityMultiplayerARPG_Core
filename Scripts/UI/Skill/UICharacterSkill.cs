using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterSkill : UIDataForCharacter<CharacterSkillTuple>
    {
        public CharacterSkill CharacterSkill { get { return Data.characterSkill; } }
        public short Level { get { return Data.targetLevel; } }
        public Skill Skill { get { return CharacterSkill != null ? CharacterSkill.GetSkill() : null; } }

        /// <summary>
        /// Format => {0} = {Title}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public string formatTitle = "{0}";
        /// <summary>
        /// Format => {0} = {Description}
        /// </summary>
        [Tooltip("Format => {0} = {Description}")]
        public string formatDescription = "{0}";
        /// <summary>
        /// Format => {0} = {Level Label}, {1} = {Level}
        /// </summary>
        [Tooltip("Format => {0} = {Level Label}, {1} = {Level}")]
        public string formatLevel = "{0}: {1}";
        /// <summary>
        /// Format => {0} = {Available Weapons Label}, {1} = {Weapon Types}
        /// </summary>
        [Tooltip("Format => {0} = {Available Weapons Label}, {1} = {Weapon Types}")]
        public string formatAvailableWeapons = "{0}: {1}";
        /// <summary>
        /// Format => {0} = {Consume Mp Label}, {1} = {Consume Mp Amount}
        /// </summary>
        [Tooltip("Format => {0} = {Consume Mp Label}, {1} = {Consume Mp Amount}")]
        public string formatConsumeMp = "{0}: {1}";
        /// <summary>
        /// Format => {0} = {Cooldown Label}, {1} = {Duration}
        /// </summary>
        [Tooltip("Format => {0} = {Cooldown Label}, {1} = {Duration}")]
        public string formatCoolDownDuration = "{0}: {1}";
        /// <summary>
        /// Format => {0} = {Remains Duration}
        /// </summary>
        [Tooltip("Format => {0} = {Remains Duration}")]
        public string formatCoolDownRemainsDuration = "{0}";
        /// <summary>
        /// Format => {0} = {Skill Type Label}, {1} = {Skill Type Title}
        /// </summary>
        [Tooltip("Format => {0} = {Skill Type Label}, {1} = {Skill Type Title}")]
        public string formatSkillType = "{0}: {1}";

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public TextWrapper uiTextLevel;
        public Image imageIcon;
        public TextWrapper uiTextSkillType;
        public TextWrapper uiTextAvailableWeapons;
        public TextWrapper uiTextConsumeMp;
        public TextWrapper uiTextCoolDownDuration;
        public TextWrapper uiTextCoolDownRemainsDuration;
        public Image imageCoolDownGage;
        public UISkillRequirement uiRequirement;
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

        private void OnDisable()
        {
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
                coolDownRemainsDuration = 0f;

            // Update UIs
            float coolDownDuration = Skill.GetCoolDownDuration(Level);

            if (uiTextCoolDownDuration != null)
            {
                uiTextCoolDownDuration.text = string.Format(
                    formatCoolDownDuration,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_SKILL_COOLDOWN.ToString()),
                    coolDownDuration.ToString("N0"));
            }

            if (uiTextCoolDownRemainsDuration != null)
            {
                uiTextCoolDownRemainsDuration.text = string.Format(formatCoolDownRemainsDuration, Mathf.CeilToInt(coolDownRemainsDuration).ToString("N0"));
                uiTextCoolDownRemainsDuration.gameObject.SetActive(coolDownRemainsDuration > 0);
            }

            if (imageCoolDownGage != null)
                imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 0 : coolDownRemainsDuration / coolDownDuration;
        }

        protected override void UpdateUI()
        {
            if (IsOwningCharacter() && CharacterSkill.CanLevelUp(OwningCharacter))
                onAbleToLevelUp.Invoke();
            else
                onUnableToLevelUp.Invoke();
        }

        protected override void UpdateData()
        {
            if (Level <= 0)
                onSetLevelZeroData.Invoke();
            else
                onSetNonLevelZeroData.Invoke();

            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(formatTitle, Skill == null ? LanguageManager.GetUnknowTitle() : Skill.Title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(formatDescription, Skill == null ? LanguageManager.GetUnknowDescription() : Skill.Description);

            if (uiTextLevel != null)
            {
                uiTextLevel.text = string.Format(
                    formatLevel,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_LEVEL.ToString()),
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
                switch (Skill.skillType)
                {
                    case SkillType.Active:
                        uiTextSkillType.text = string.Format(
                            formatSkillType,
                            LanguageManager.GetText(UILocaleKeys.UI_LABEL_SKILL_TYPE.ToString()),
                            LanguageManager.GetText(UILocaleKeys.UI_SKILL_TYPE_ACTIVE.ToString()));
                        break;
                    case SkillType.Passive:
                        uiTextSkillType.text = string.Format(
                            formatSkillType,
                            LanguageManager.GetText(UILocaleKeys.UI_LABEL_SKILL_TYPE.ToString()),
                            LanguageManager.GetText(UILocaleKeys.UI_SKILL_TYPE_PASSIVE.ToString()));
                        break;
                    case SkillType.CraftItem:
                        uiTextSkillType.text = string.Format(
                            formatSkillType,
                            LanguageManager.GetText(UILocaleKeys.UI_LABEL_SKILL_TYPE.ToString()),
                            LanguageManager.GetText(UILocaleKeys.UI_SKILL_TYPE_CRAFT_ITEM.ToString()));
                        break;
                }
            }

            if (uiTextAvailableWeapons != null)
            {
                if (Skill.availableWeapons == null || Skill.availableWeapons.Length == 0)
                {
                    uiTextAvailableWeapons.gameObject.SetActive(false);
                }
                else
                {
                    string str = string.Empty;
                    foreach (WeaponType availableWeapon in Skill.availableWeapons)
                    {
                        if (!string.IsNullOrEmpty(str))
                            str += "/";
                        str += availableWeapon.Title;
                    }
                    uiTextAvailableWeapons.text = string.Format(
                        formatAvailableWeapons,
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_AVAILABLE_WEAPONS.ToString()),
                        str);
                    uiTextAvailableWeapons.gameObject.SetActive(true);
                }
            }

            if (uiTextConsumeMp != null)
            {
                uiTextConsumeMp.text = string.Format(
                    formatConsumeMp,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_CONSUME_MP.ToString()),
                    (Skill == null || Level <= 0) ?
                        LanguageManager.GetUnknowDescription() :
                        Skill.GetConsumeMp(Level).ToString("N0"));
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
                    uiRequirement.Data = new CharacterSkillTuple(CharacterSkill, Level);
                }
            }

            if (uiCraftItem != null)
            {
                if (Skill == null || Skill.skillType != SkillType.CraftItem)
                    uiCraftItem.Hide();
                else
                {
                    uiCraftItem.SetupForCharacter(Skill.itemCraft);
                    uiCraftItem.Show();
                }
            }

            bool isAttack = Skill != null && Skill.IsAttack();
            bool isOverrideWeaponDamage = isAttack && Skill.skillAttackType == SkillAttackType.Normal;
            if (uiDamageAmount != null)
            {
                if (!isOverrideWeaponDamage)
                    uiDamageAmount.Hide();
                else
                {
                    uiDamageAmount.Show();
                    KeyValuePair<DamageElement, MinMaxFloat> keyValuePair = Skill.GetDamageAmount(Level, null);
                    uiDamageAmount.Data = new DamageElementAmountTuple(keyValuePair.Key, keyValuePair.Value);
                }
            }

            if (uiDamageInflictions != null)
            {
                Dictionary<DamageElement, float> damageInflictionRates = Skill.GetWeaponDamageInflictions(Level);
                if (!isAttack || damageInflictionRates == null || damageInflictionRates.Count == 0)
                    uiDamageInflictions.Hide();
                else
                {
                    uiDamageInflictions.Show();
                    uiDamageInflictions.Data = damageInflictionRates;
                }
            }

            if (uiAdditionalDamageAmounts != null)
            {
                Dictionary<DamageElement, MinMaxFloat> additionalDamageAmounts = Skill.GetAdditionalDamageAmounts(Level);
                if (!isAttack || additionalDamageAmounts == null || additionalDamageAmounts.Count == 0)
                    uiAdditionalDamageAmounts.Hide();
                else
                {
                    uiAdditionalDamageAmounts.Show();
                    uiAdditionalDamageAmounts.Data = additionalDamageAmounts;
                }
            }

            if (uiSkillBuff != null)
            {
                if (!Skill.IsBuff())
                    uiSkillBuff.Hide();
                else
                {
                    uiSkillBuff.Show();
                    uiSkillBuff.Data = new BuffTuple(Skill.buff, Level);
                }
            }

            if (uiSkillDebuff != null)
            {
                if (!Skill.IsDebuff())
                    uiSkillDebuff.Hide();
                else
                {
                    uiSkillDebuff.Show();
                    uiSkillDebuff.Data = new BuffTuple(Skill.debuff, Level);
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
                    uiNextLevelSkill.Hide();
                else
                {
                    uiNextLevelSkill.Setup(new CharacterSkillTuple(CharacterSkill, (short)(Level + 1)), Character, IndexOfData);
                    uiNextLevelSkill.Show();
                }
            }
        }

        public void OnClickAdd()
        {
            OwningCharacter.RequestAddSkill(Skill.DataId);
        }
    }
}
