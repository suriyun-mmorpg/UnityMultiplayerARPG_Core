using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UICharacterSkill : UIDataForCharacter<CharacterSkillLevelTuple>
    {
        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";
        [Tooltip("Level Format => {0} = {Level}")]
        public string levelFormat = "Lv: {0}";
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
        public Text textTitle;
        public Text textDescription;
        public Text textLevel;
        public Image imageIcon;
        public Text textSkillType;
        public Text textConsumeMp;
        public Text textCoolDownDuration;
        public Text textCoolDownRemainsDuration;
        public Image imageCoolDownGage;
        public UISkillRequirement uiRequirement;
        public UISkillCraftItem uiCraftItem;

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
        public UICharacterSkill uiNextLevelSkill;
        public bool hideRemainsDurationWhenIsZero;

        protected float collectedDeltaTime;

        protected void Update()
        {
            var characterSkill = Data.characterSkill;
            var skill = characterSkill.GetSkill();
            var level = Data.targetLevel;

            collectedDeltaTime += Time.deltaTime;

            if (IsOwningCharacter() && characterSkill.CanLevelUp(BasePlayerCharacterController.OwningCharacter))
                onAbleToLevelUp.Invoke();
            else
                onUnableToLevelUp.Invoke();

            var coolDownRemainsDuration = characterSkill.coolDownRemainsDuration - collectedDeltaTime;
            if (coolDownRemainsDuration < 0)
                coolDownRemainsDuration = 0;
            var coolDownDuration = skill.GetCoolDownDuration(level);

            if (textCoolDownDuration != null)
                textCoolDownDuration.text = string.Format(coolDownDurationFormat, coolDownDuration.ToString("N0"));

            if (textCoolDownRemainsDuration != null)
            {
                var remainsDurationString = "";
                if (!hideRemainsDurationWhenIsZero || coolDownRemainsDuration > 0)
                {
                    if (skill == null)
                        remainsDurationString = string.Format(coolDownRemainsDurationFormat, "0");
                    else
                        remainsDurationString = string.Format(coolDownRemainsDurationFormat, Mathf.CeilToInt(coolDownRemainsDuration).ToString("N0"));
                }
                textCoolDownRemainsDuration.text = remainsDurationString;
            }

            if (imageCoolDownGage != null)
                imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 0 : coolDownRemainsDuration / coolDownDuration;
        }

        protected override void UpdateData()
        {
            var characterSkill = Data.characterSkill;
            var skill = characterSkill.GetSkill();
            short level = Data.targetLevel;

            collectedDeltaTime = 0f;

            if (level <= 0)
                onSetLevelZeroData.Invoke();
            else
                onSetNonLevelZeroData.Invoke();

            if (textTitle != null)
                textTitle.text = string.Format(titleFormat, skill == null ? "Unknow" : skill.title);

            if (textDescription != null)
                textDescription.text = string.Format(descriptionFormat, skill == null ? "N/A" : skill.description);

            if (textLevel != null)
                textLevel.text = string.Format(levelFormat, level.ToString("N0"));

            if (imageIcon != null)
            {
                var iconSprite = skill == null ? null : skill.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (textSkillType != null)
            {
                switch (skill.skillType)
                {
                    case SkillType.Active:
                        textSkillType.text = string.Format(skillTypeFormat, activeSkillType);
                        break;
                    case SkillType.Passive:
                        textSkillType.text = string.Format(skillTypeFormat, passiveSkillType);
                        break;
                    case SkillType.CraftItem:
                        textSkillType.text = string.Format(skillTypeFormat, craftItemSkillType);
                        break;
                }
            }

            if (textConsumeMp != null)
                textConsumeMp.text = string.Format(consumeMpFormat, skill == null || level <= 0 ? "N/A" : skill.GetConsumeMp(level).ToString("N0"));

            if (uiRequirement != null)
            {
                if (skill == null || (skill.GetRequireCharacterLevel(level) == 0 && skill.CacheRequireSkillLevels.Count == 0))
                    uiRequirement.Hide();
                else
                {
                    uiRequirement.Show();
                    uiRequirement.Data = new SkillLevelTuple(skill, level);
                }
            }

            if (uiCraftItem != null)
            {
                if (skill == null || skill.skillType != SkillType.CraftItem)
                    uiCraftItem.Hide();
                else
                {
                    uiCraftItem.Show();
                    uiCraftItem.Data = skill;
                }
            }

            var isAttack = skill != null && skill.IsAttack();
            var isOverrideWeaponDamage = isAttack && skill.skillAttackType == SkillAttackType.Normal;
            if (uiDamageAmount != null)
            {
                if (!isOverrideWeaponDamage)
                    uiDamageAmount.Hide();
                else
                {
                    uiDamageAmount.Show();
                    var keyValuePair = skill.GetDamageAmount(level, null);
                    uiDamageAmount.Data = new DamageElementAmountTuple(keyValuePair.Key, keyValuePair.Value);
                }
            }

            if (uiDamageInflictions != null)
            {
                var damageInflictionRates = skill.GetWeaponDamageInflictions(level);
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
                var additionalDamageAmounts = skill.GetAdditionalDamageAmounts(level);
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
                if (!skill.IsBuff())
                    uiSkillBuff.Hide();
                else
                {
                    uiSkillBuff.Show();
                    uiSkillBuff.Data = new BuffLevelTuple(skill.buff, level);
                }
            }

            if (uiSkillDebuff != null)
            {
                if (!skill.IsDebuff())
                    uiSkillDebuff.Hide();
                else
                {
                    uiSkillDebuff.Show();
                    uiSkillDebuff.Data = new BuffLevelTuple(skill.debuff, level);
                }
            }

            if (uiNextLevelSkill != null)
            {
                if (level + 1 > skill.maxLevel)
                    uiNextLevelSkill.Hide();
                else
                {
                    uiNextLevelSkill.Setup(new CharacterSkillLevelTuple(characterSkill, (short)(level + 1)), character, indexOfData);
                    uiNextLevelSkill.Show();
                }
            }
        }

        public void OnClickAdd()
        {
            if (!IsOwningCharacter())
                return;

            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestAddSkill(indexOfData, 1);
        }
    }

    [System.Serializable]
    public class UICharacterSkillEvent : UnityEvent<UICharacterSkill> { }
}
