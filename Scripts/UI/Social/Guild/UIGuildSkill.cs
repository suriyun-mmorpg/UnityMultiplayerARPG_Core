using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIGuildSkill : UISelectionEntry<UIGuildSkillData>
    {
        public GuildSkill GuildSkill { get { return Data.guildSkill; } }
        public short Level { get { return Data.targetLevel; } }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Description}")]
        public UILocaleKeySetting formatKeyDescription = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {Duration}")]
        public UILocaleKeySetting formatKeyCoolDownDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_COOLDOWN_DURATION);
        [Tooltip("Format => {0} = {Remains Duration}")]
        public UILocaleKeySetting formatKeyCoolDownRemainsDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Skill Type Title}")]
        public UILocaleKeySetting formatKeySkillType = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_TYPE);

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
            {
                coolDownRemainsDuration = 0f;
            }

            // Update UIs
            float coolDownDuration = GuildSkill.GetCoolDownDuration(Level);

            if (uiTextCoolDownDuration != null)
            {
                uiTextCoolDownDuration.text = string.Format(
                    LanguageManager.GetText(formatKeyCoolDownDuration),
                    coolDownDuration.ToString("N0"));
            }

            if (uiTextCoolDownRemainsDuration != null)
            {
                uiTextCoolDownRemainsDuration.text = string.Format(
                    LanguageManager.GetText(formatKeyCoolDownRemainsDuration),
                    coolDownRemainsDuration.ToString("N0"));
                uiTextCoolDownRemainsDuration.gameObject.SetActive(coolDownRemainsDuration > 0);
            }

            if (imageCoolDownGage != null)
            {
                imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 0 : coolDownRemainsDuration / coolDownDuration;
            }
        }

        protected override void UpdateUI()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null && Level < GuildSkill.maxLevel &&
                BaseGameNetworkManager.ClientGuild != null &&
                BaseGameNetworkManager.ClientGuild.IsLeader(owningCharacter) &&
                BaseGameNetworkManager.ClientGuild.skillPoint > 0)
            {
                onAbleToLevelUp.Invoke();
            }
            else
            {
                onUnableToLevelUp.Invoke();
            }

            if (owningCharacter != null && Level > 1 &&
                GuildSkill.GetSkillType() == GuildSkillType.Active)
            {
                onAbleToUse.Invoke();
            }
            else
            {
                onUnableToUse.Invoke();
            }
        }

        protected override void UpdateData()
        {
            if (Level <= 0)
                onSetLevelZeroData.Invoke();
            else
                onSetNonLevelZeroData.Invoke();

            if (uiTextTitle != null)
            {
                uiTextTitle.text = string.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    GuildSkill == null ? LanguageManager.GetUnknowTitle() : GuildSkill.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = string.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    GuildSkill == null ? LanguageManager.GetUnknowDescription() : GuildSkill.Description);
            }

            if (uiTextLevel != null)
            {
                uiTextLevel.text = string.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Level.ToString("N0"));
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = GuildSkill == null ? null : GuildSkill.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiTextSkillType != null)
            {
                switch (GuildSkill.GetSkillType())
                {
                    case GuildSkillType.Active:
                        uiTextSkillType.text = string.Format(
                            LanguageManager.GetText(formatKeySkillType),
                            LanguageManager.GetText(UITextKeys.UI_SKILL_TYPE_ACTIVE.ToString()));
                        break;
                    case GuildSkillType.Passive:
                        uiTextSkillType.text = string.Format(
                            LanguageManager.GetText(formatKeySkillType),
                            LanguageManager.GetText(UITextKeys.UI_SKILL_TYPE_PASSIVE.ToString()));
                        break;
                }
            }

            if (uiTextIncreaseMaxMember != null)
            {
                int amount = GuildSkill.GetIncreaseMaxMember(Level);
                uiTextIncreaseMaxMember.text = string.Format(
                    LanguageManager.GetText(UIFormatKeys.UI_FORMAT_INCREASE_MAX_MEMBER.ToString()),
                    amount.ToString("N0"));
                uiTextIncreaseMaxMember.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseExpGainPercentage != null)
            {
                float amount = GuildSkill.GetIncreaseExpGainPercentage(Level);
                uiTextIncreaseExpGainPercentage.text = string.Format(
                    LanguageManager.GetText(UIFormatKeys.UI_FORMAT_INCREASE_EXP_GAIN_PERCENTAGE.ToString()),
                    amount.ToString("N2"));
                uiTextIncreaseExpGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseGoldGainPercentage != null)
            {
                float amount = GuildSkill.GetIncreaseGoldGainPercentage(Level);
                uiTextIncreaseGoldGainPercentage.text = string.Format(
                    LanguageManager.GetText(UIFormatKeys.UI_FORMAT_INCREASE_GOLD_GAIN_PERCENTAGE.ToString()),
                    amount.ToString("N2"));
                uiTextIncreaseGoldGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseShareExpGainPercentage != null)
            {
                float amount = GuildSkill.GetIncreaseShareExpGainPercentage(Level);
                uiTextIncreaseShareExpGainPercentage.text = string.Format(
                    LanguageManager.GetText(UIFormatKeys.UI_FORMAT_INCREASE_SHARE_EXP_GAIN_PERCENTAGE.ToString()),
                    amount.ToString("N2"));
                uiTextIncreaseShareExpGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseShareGoldGainPercentage != null)
            {
                float amount = GuildSkill.GetIncreaseShareGoldGainPercentage(Level);
                uiTextIncreaseShareGoldGainPercentage.text = string.Format(
                    LanguageManager.GetText(UIFormatKeys.UI_FORMAT_INCREASE_SHARE_GOLD_GAIN_PERCENTAGE.ToString()),
                    amount.ToString("N2"));
                uiTextIncreaseShareGoldGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextDecreaseExpLostPercentage != null)
            {
                float amount = GuildSkill.GetDecreaseExpLostPercentage(Level);
                uiTextDecreaseExpLostPercentage.text = string.Format(
                    LanguageManager.GetText(UIFormatKeys.UI_FORMAT_DECREASE_EXP_PENALTY_PERCENTAGE.ToString()),
                    amount.ToString("N2"));
                uiTextDecreaseExpLostPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiSkillBuff != null)
            {
                if (!GuildSkill.IsBuff())
                {
                    uiSkillBuff.Hide();
                }
                else
                {
                    uiSkillBuff.Show();
                    uiSkillBuff.Data = new UIBuffData(GuildSkill.GetBuff(), Level);
                }
            }

            if (uiNextLevelSkill != null)
            {
                if (Level + 1 > GuildSkill.maxLevel)
                {
                    uiNextLevelSkill.Hide();
                }
                else
                {
                    uiNextLevelSkill.Data = new UIGuildSkillData(GuildSkill, (short)(Level + 1));
                    uiNextLevelSkill.Show();
                }
            }
        }

        public void OnClickAdd()
        {
            if (BaseGameNetworkManager.ClientGuild == null)
                return;
            BasePlayerCharacterController.OwningCharacter.RequestAddGuildSkill(GuildSkill.DataId);
        }

        public void OnClickUse()
        {
            if (BaseGameNetworkManager.ClientGuild == null)
                return;
            BasePlayerCharacterController.OwningCharacter.RequestUseGuildSkill(GuildSkill.DataId);
        }
    }
}
