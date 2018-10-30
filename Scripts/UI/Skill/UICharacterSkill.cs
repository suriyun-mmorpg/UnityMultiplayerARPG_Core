using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterSkill : UIDataForCharacter<SkillTuple>
    {
        public Skill Skill { get { return Data.skill; } }
        public short Level { get { return Data.targetLevel; } }

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
        public UICharacterSkill uiNextLevelSkill;

        protected float coolDownRemainsDuration;

        private void OnDisable()
        {
            coolDownRemainsDuration = 0f;
        }

        protected override void Update()
        {
            base.Update();

            if (IsOwningCharacter() && Skill.CanLevelUp(BasePlayerCharacterController.OwningCharacter, Level))
                onAbleToLevelUp.Invoke();
            else
                onUnableToLevelUp.Invoke();

            if (coolDownRemainsDuration <= 0f)
            {
                if (character != null && Skill != null)
                {
                    var indexOfSkillUsage = character.IndexOfSkillUsage(Skill.DataId, SkillUsageType.Skill);
                    if (indexOfSkillUsage >= 0)
                    {
                        coolDownRemainsDuration = character.SkillUsages[indexOfSkillUsage].coolDownRemainsDuration;
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
            var coolDownDuration = Skill.GetCoolDownDuration(Level);

            if (uiTextCoolDownDuration != null)
                uiTextCoolDownDuration.text = string.Format(coolDownDurationFormat, coolDownDuration.ToString("N0"));

            if (uiTextCoolDownRemainsDuration != null)
            {
                if (coolDownRemainsDuration > 0f)
                    uiTextCoolDownRemainsDuration.text = string.Format(coolDownRemainsDurationFormat, Mathf.CeilToInt(coolDownRemainsDuration).ToString("N0"));
                else
                    uiTextCoolDownRemainsDuration.text = "";
            }

            if (imageCoolDownGage != null)
                imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 0 : coolDownRemainsDuration / coolDownDuration;
        }

        protected override void UpdateData()
        {
            if (Level <= 0)
                onSetLevelZeroData.Invoke();
            else
                onSetNonLevelZeroData.Invoke();

            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(titleFormat, Skill == null ? "Unknow" : Skill.title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, Skill == null ? "N/A" : Skill.description);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, Level.ToString("N0"));

            if (imageIcon != null)
            {
                var iconSprite = Skill == null ? null : Skill.icon;
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
                    var str = string.Empty;
                    foreach (var availableWeapon in Skill.availableWeapons)
                    {
                        if (!string.IsNullOrEmpty(str))
                            str += "/";
                        str += availableWeapon.title;
                    }
                    uiTextAvailableWeapons.text = string.Format(availableWeaponsFormat, str);
                    uiTextAvailableWeapons.gameObject.SetActive(true);
                }
            }

            if (uiTextConsumeMp != null)
                uiTextConsumeMp.text = string.Format(consumeMpFormat, Skill == null || Level <= 0 ? "N/A" : Skill.GetConsumeMp(Level).ToString("N0"));

            if (uiRequirement != null)
            {
                if (Skill == null || (Skill.GetRequireCharacterLevel(Level) == 0 && Skill.CacheRequireSkillLevels.Count == 0))
                    uiRequirement.Hide();
                else
                {
                    uiRequirement.Show();
                    uiRequirement.Data = new SkillTuple(Skill, Level);
                }
            }

            if (uiCraftItem != null)
            {
                if (Skill == null || Skill.skillType != SkillType.CraftItem)
                    uiCraftItem.Hide();
                else
                {
                    uiCraftItem.Show();
                    uiCraftItem.Data = Skill.itemCraft;
                }
            }

            var isAttack = Skill != null && Skill.IsAttack();
            var isOverrideWeaponDamage = isAttack && Skill.skillAttackType == SkillAttackType.Normal;
            if (uiDamageAmount != null)
            {
                if (!isOverrideWeaponDamage)
                    uiDamageAmount.Hide();
                else
                {
                    uiDamageAmount.Show();
                    var keyValuePair = Skill.GetDamageAmount(Level, null);
                    uiDamageAmount.Data = new DamageElementAmountTuple(keyValuePair.Key, keyValuePair.Value);
                }
            }

            if (uiDamageInflictions != null)
            {
                var damageInflictionRates = Skill.GetWeaponDamageInflictions(Level);
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
                var additionalDamageAmounts = Skill.GetAdditionalDamageAmounts(Level);
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

            if (uiNextLevelSkill != null)
            {
                if (Level + 1 > Skill.maxLevel)
                    uiNextLevelSkill.Hide();
                else
                {
                    uiNextLevelSkill.Setup(new SkillTuple(Skill, (short)(Level + 1)), character, indexOfData);
                    uiNextLevelSkill.Show();
                }
            }
        }

        public void OnClickAdd()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
                return;

            owningCharacter.RequestAddSkill(Skill.DataId);
        }
    }
}
