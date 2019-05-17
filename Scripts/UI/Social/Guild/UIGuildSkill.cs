using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIGuildSkill : UISelectionEntry<GuildSkillTuple>
    {
        public GuildSkill GuildSkill { get { return Data.guildSkill; } }
        public short Level { get { return Data.targetLevel; } }

        [Header("Display Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";
        [Tooltip("Level Format => {0} = {Level}, {1} = {Level Label}")]
        public string levelFormat = "{1}: {0}";
        [Tooltip("Cool Down Duration Format => {0} = {Duration}, {1} = {Cooldown Label}")]
        public string coolDownDurationFormat = "{1}: {0}";
        [Tooltip("Cool Down Remains Duration Format => {0} = {Remains duration}")]
        public string coolDownRemainsDurationFormat = "{0}";
        [Tooltip("Skill Type Format => {0} = {Skill Type title}, {1} = {Skill Type Label}")]
        public string skillTypeFormat = "{1}: {0}";

        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public TextWrapper uiTextLevel;
        public Image imageIcon;
        public TextWrapper uiTextSkillType;
        public TextWrapper uiTextCoolDownDuration;
        public TextWrapper uiTextCoolDownRemainsDuration;
        public Image imageCoolDownGage;

        [Header("Passive Bonus")]
        public TextWrapper uiTextIncreaseMaxMember;
        public TextWrapper uiTextIncreaseExpGainPercentage;
        public TextWrapper uiTextIncreaseGoldGainPercentage;
        public TextWrapper uiTextIncreaseShareExpGainPercentage;
        public TextWrapper uiTextIncreaseShareGoldGainPercentage;
        public TextWrapper uiTextDecreaseExpLostPercentage;

        [Header("Buff")]
        public UIBuff uiSkillBuff;

        [Header("Events")]
        public UnityEvent onSetLevelZeroData;
        public UnityEvent onSetNonLevelZeroData;
        public UnityEvent onAbleToLevelUp;
        public UnityEvent onUnableToLevelUp;
        public UnityEvent onAbleToUse;
        public UnityEvent onUnableToUse;

        [Header("Options")]
        public UIGuildSkill uiNextLevelSkill;

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
                BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
                if (owningCharacter != null && GuildSkill != null)
                {
                    int indexOfSkillUsage = owningCharacter.IndexOfSkillUsage(GuildSkill.DataId, SkillUsageType.GuildSkill);
                    if (indexOfSkillUsage >= 0)
                    {
                        coolDownRemainsDuration = owningCharacter.SkillUsages[indexOfSkillUsage].coolDownRemainsDuration;
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
            float coolDownDuration = GuildSkill.GetCoolDownDuration(Level);

            if (uiTextCoolDownDuration != null)
                uiTextCoolDownDuration.text = string.Format(coolDownDurationFormat, coolDownDuration.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LABEL_SKILL_COOLDOWN.ToString()));

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
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null &&
                Level < GuildSkill.GetMaxLevel() &&
                owningCharacter.gameManager.ClientGuild != null &&
                owningCharacter.gameManager.ClientGuild.IsLeader(owningCharacter) &&
                owningCharacter.gameManager.ClientGuild.skillPoint > 0)
                onAbleToLevelUp.Invoke();
            else
                onUnableToLevelUp.Invoke();

            if (owningCharacter != null &&
                Level > 1 &&
                GuildSkill.skillType == GuildSkillType.Active)
                onAbleToUse.Invoke();
            else
                onUnableToUse.Invoke();
        }

        protected override void UpdateData()
        {
            if (Level <= 0)
                onSetLevelZeroData.Invoke();
            else
                onSetNonLevelZeroData.Invoke();

            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(titleFormat, GuildSkill == null ? LanguageManager.GetUnknowTitle() : GuildSkill.Title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, GuildSkill == null ? LanguageManager.GetUnknowDescription() : GuildSkill.Description);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, Level.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LABEL_LEVEL.ToString()));

            if (imageIcon != null)
            {
                Sprite iconSprite = GuildSkill == null ? null : GuildSkill.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiTextSkillType != null)
            {
                switch (GuildSkill.skillType)
                {
                    case GuildSkillType.Active:
                        uiTextSkillType.text = string.Format(skillTypeFormat, LanguageManager.GetText(UILocaleKeys.UI_SKILL_TYPE_ACTIVE.ToString()), LanguageManager.GetText(UILocaleKeys.UI_LABEL_SKILL_TYPE.ToString()));
                        break;
                    case GuildSkillType.Passive:
                        uiTextSkillType.text = string.Format(skillTypeFormat, LanguageManager.GetText(UILocaleKeys.UI_SKILL_TYPE_PASSIVE.ToString()), LanguageManager.GetText(UILocaleKeys.UI_LABEL_SKILL_TYPE.ToString()));
                        break;
                }
            }

            if (uiTextIncreaseMaxMember != null)
            {
                int amount = GuildSkill.increaseMaxMember.GetAmount(Level);
                uiTextIncreaseMaxMember.text = string.Format(
                    LanguageManager.GetText(UILocaleKeys.UI_FORMAT_INCREASE_MAX_MEMBER.ToString()),
                    amount.ToString("N0"));
                uiTextIncreaseMaxMember.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseExpGainPercentage != null)
            {
                float amount = GuildSkill.increaseExpGainPercentage.GetAmount(Level);
                uiTextIncreaseExpGainPercentage.text = string.Format(
                    LanguageManager.GetText(UILocaleKeys.UI_FORMAT_INCREASE_EXP_GAIN_PERCENTAGE.ToString()),
                    amount.ToString("N2"));
                uiTextIncreaseExpGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseGoldGainPercentage != null)
            {
                float amount = GuildSkill.increaseGoldGainPercentage.GetAmount(Level);
                uiTextIncreaseGoldGainPercentage.text = string.Format(
                    LanguageManager.GetText(UILocaleKeys.UI_FORMAT_INCREASE_GOLD_GAIN_PERCENTAGE.ToString()),
                    amount.ToString("N2"));
                uiTextIncreaseGoldGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseShareExpGainPercentage != null)
            {
                float amount = GuildSkill.increaseShareExpGainPercentage.GetAmount(Level);
                uiTextIncreaseShareExpGainPercentage.text = string.Format(
                    LanguageManager.GetText(UILocaleKeys.UI_FORMAT_INCREASE_SHARE_EXP_GAIN_PERCENTAGE.ToString()),
                    amount.ToString("N2"));
                uiTextIncreaseShareExpGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseShareGoldGainPercentage != null)
            {
                float amount = GuildSkill.increaseShareGoldGainPercentage.GetAmount(Level);
                uiTextIncreaseShareGoldGainPercentage.text = string.Format(
                    LanguageManager.GetText(UILocaleKeys.UI_FORMAT_INCREASE_SHARE_GOLD_GAIN_PERCENTAGE.ToString()),
                    amount.ToString("N2"));
                uiTextIncreaseShareGoldGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextDecreaseExpLostPercentage != null)
            {
                float amount = GuildSkill.decreaseExpLostPercentage.GetAmount(Level);
                uiTextDecreaseExpLostPercentage.text = string.Format(
                    LanguageManager.GetText(UILocaleKeys.UI_FORMAT_DECREASE_EXP_PENALTY_PERCENTAGE.ToString()),
                    amount.ToString("N2"));
                uiTextDecreaseExpLostPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiSkillBuff != null)
            {
                if (!GuildSkill.IsBuff())
                    uiSkillBuff.Hide();
                else
                {
                    uiSkillBuff.Show();
                    uiSkillBuff.Data = new BuffTuple(GuildSkill.buff, Level);
                }
            }

            if (uiNextLevelSkill != null)
            {
                if (Level + 1 > GuildSkill.maxLevel)
                    uiNextLevelSkill.Hide();
                else
                {
                    uiNextLevelSkill.Data = new GuildSkillTuple(GuildSkill, (short)(Level + 1));
                    uiNextLevelSkill.Show();
                }
            }
        }

        public void OnClickAdd()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null || owningCharacter.gameManager.ClientGuild == null)
                return;

            if (owningCharacter != null)
                owningCharacter.RequestAddGuildSkill(GuildSkill.DataId);
        }

        public void OnClickUse()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null || owningCharacter.gameManager.ClientGuild == null)
                return;

            owningCharacter.RequestUseGuildSkill(GuildSkill.DataId);
        }
    }
}
