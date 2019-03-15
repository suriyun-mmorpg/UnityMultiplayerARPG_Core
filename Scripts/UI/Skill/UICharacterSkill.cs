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

        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";
        [Tooltip("Level Format => {0} = {Level}")]
        public string levelFormat = "Lv: {0}";
        [Tooltip("Available Weapons Format => {0} = {Weapon Types}")]
        public string availableWeaponsFormat = "Available Weapons: {0}";
        [Tooltip("Consume Mp Format => {0} = {Consume Mp amount}")]
        public string consumeMpFormat = "Consume Mp: {0}";
        [Tooltip("Cool Down Duration Format => {0} = {Duration}")]
        public string coolDownDurationFormat = "Cooldown: {0}";
        [Tooltip("Cool Down Remains Duration Format => {0} = {Remains duration}")]
        public string coolDownRemainsDurationFormat = "{0}";
        [Tooltip("Skill Type Format => {0} = {Skill Type title}")]
        public string skillTypeFormat = "Skill Type: {0}";
        [Tooltip("Active Skill Type")]
        public string activeSkillType = "Active";
        [Tooltip("Passive Skill Type")]
        public string passiveSkillType = "Passive";
        [Tooltip("Craft Item Skill Type")]
        public string craftItemSkillType = "Craft Item";

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
                uiTextCoolDownDuration.text = string.Format(coolDownDurationFormat, coolDownDuration.ToString("N0"));

            if (uiTextCoolDownRemainsDuration != null)
            {
                uiTextCoolDownRemainsDuration.text = string.Format(coolDownRemainsDurationFormat, Mathf.CeilToInt(coolDownRemainsDuration).ToString("N0"));
                uiTextCoolDownRemainsDuration.gameObject.SetActive(coolDownRemainsDuration > 0);
            }

            if (imageCoolDownGage != null)
                imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 0 : coolDownRemainsDuration / coolDownDuration;
        }

        protected override void UpdateUI()
        {
            if (IsOwningCharacter() && Skill.CanLevelUp(OwningCharacter, Level))
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
                uiTextTitle.text = string.Format(titleFormat, Skill == null ? LanguageManager.GetUnknowTitle() : Skill.Title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, Skill == null ? LanguageManager.GetUnknowDescription() : Skill.Description);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, Level.ToString("N0"));

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
                        uiTextSkillType.text = string.Format(skillTypeFormat, activeSkillType);
                        break;
                    case SkillType.Passive:
                        uiTextSkillType.text = string.Format(skillTypeFormat, passiveSkillType);
                        break;
                    case SkillType.CraftItem:
                        uiTextSkillType.text = string.Format(skillTypeFormat, craftItemSkillType);
                        break;
                }
            }

            if (uiTextAvailableWeapons != null)
            {
                if (Skill.availableWeapons == null || Skill.availableWeapons.Length == 0)
                    uiTextAvailableWeapons.gameObject.SetActive(false);
                else
                {
                    string str = string.Empty;
                    foreach (WeaponType availableWeapon in Skill.availableWeapons)
                    {
                        if (!string.IsNullOrEmpty(str))
                            str += "/";
                        str += availableWeapon.Title;
                    }
                    uiTextAvailableWeapons.text = string.Format(availableWeaponsFormat, str);
                    uiTextAvailableWeapons.gameObject.SetActive(true);
                }
            }

            if (uiTextConsumeMp != null)
                uiTextConsumeMp.text = string.Format(consumeMpFormat, Skill == null || Level <= 0 ? LanguageManager.GetUnknowDescription() : Skill.GetConsumeMp(Level).ToString("N0"));

            if (uiRequirement != null)
            {
                if (Skill == null || (Skill.GetRequireCharacterLevel(Level) == 0 && Skill.CacheRequireSkillLevels.Count == 0))
                    uiRequirement.Hide();
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
