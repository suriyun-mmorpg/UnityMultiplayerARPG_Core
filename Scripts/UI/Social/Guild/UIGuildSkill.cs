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
        [Tooltip("Level Format => {0} = {Level}")]
        public string levelFormat = "Lv: {0}";
        [Tooltip("Cool Down Duration Format => {0} = {Duration}")]
        public string coolDownDurationFormat = "Cooldown: {0}";
        [Tooltip("Cool Down Remains Duration Format => {0} = {Remains duration}")]
        public string coolDownRemainsDurationFormat = "{0}";
        [Tooltip("Increase Max Member Format")]
        public string increaseMaxMemberFormat = "Max Member +{0}";
        [Tooltip("Increase Exp Gain Percentage Format")]
        public string increaseExpGainPercentageFormat = "Exp Gain +{0}%";
        [Tooltip("Increase Gold Gain Percentage Format")]
        public string increaseGoldGainPercentageFormat = "Gold Gain +{0}%";
        [Tooltip("Increase Share Exp Gain Percentage Format")]
        public string increaseShareExpGainPercentageFormat = "Party Share Exp +{0}%";
        [Tooltip("Increase Share Gold Gain Percentage Format")]
        public string increaseShareGoldGainPercentageFormat = "Party Share Gold +{0}%";
        [Tooltip("Decrease Exp Lost Percentage Format")]
        public string decreaseExpLostPercentageFormat = "Exp Penalty -{0}%";
        [Tooltip("Skill Type Format => {0} = {Skill Type title}")]
        public string skillTypeFormat = "Skill Type: {0}";
        [Tooltip("Active Skill Type")]
        public string activeSkillType = "Active";
        [Tooltip("Passive Skill Type")]
        public string passiveSkillType = "Passive";

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

        protected override void UpdateUI()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null &&
                Level < GuildSkill.GetMaxLevel() &&
                owningCharacter.GameManager.ClientGuild != null &&
                owningCharacter.GameManager.ClientGuild.IsLeader(owningCharacter) &&
                owningCharacter.GameManager.ClientGuild.skillPoint > 0)
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
                uiTextTitle.text = string.Format(titleFormat, GuildSkill == null ? "Unknow" : GuildSkill.title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, GuildSkill == null ? "N/A" : GuildSkill.description);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, Level.ToString("N0"));

            if (imageIcon != null)
            {
                var iconSprite = GuildSkill == null ? null : GuildSkill.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiTextSkillType != null)
            {
                switch (GuildSkill.skillType)
                {
                    case GuildSkillType.Active:
                        uiTextSkillType.text = string.Format(skillTypeFormat, activeSkillType);
                        break;
                    case GuildSkillType.Passive:
                        uiTextSkillType.text = string.Format(skillTypeFormat, passiveSkillType);
                        break;
                }
            }

            if (uiTextIncreaseMaxMember != null)
            {
                var amount = GuildSkill.increaseMaxMember.GetAmount(Level);
                uiTextIncreaseMaxMember.text = string.Format(increaseMaxMemberFormat, amount.ToString("N0"));
                uiTextIncreaseMaxMember.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseExpGainPercentage != null)
            {
                var amount = GuildSkill.increaseExpGainPercentage.GetAmount(Level);
                uiTextIncreaseExpGainPercentage.text = string.Format(increaseExpGainPercentageFormat, amount.ToString("N2"));
                uiTextIncreaseExpGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseGoldGainPercentage != null)
            {
                var amount = GuildSkill.increaseGoldGainPercentage.GetAmount(Level);
                uiTextIncreaseGoldGainPercentage.text = string.Format(increaseGoldGainPercentageFormat, amount.ToString("N2"));
                uiTextIncreaseGoldGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseShareExpGainPercentage != null)
            {
                var amount = GuildSkill.increaseShareExpGainPercentage.GetAmount(Level);
                uiTextIncreaseShareExpGainPercentage.text = string.Format(increaseShareExpGainPercentageFormat, amount.ToString("N2"));
                uiTextIncreaseShareExpGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextIncreaseShareGoldGainPercentage != null)
            {
                var amount = GuildSkill.increaseShareGoldGainPercentage.GetAmount(Level);
                uiTextIncreaseShareGoldGainPercentage.text = string.Format(increaseShareGoldGainPercentageFormat, amount.ToString("N2"));
                uiTextIncreaseShareGoldGainPercentage.gameObject.SetActive(amount != 0);
            }

            if (uiTextDecreaseExpLostPercentage != null)
            {
                var amount = GuildSkill.decreaseExpLostPercentage.GetAmount(Level);
                uiTextDecreaseExpLostPercentage.text = string.Format(decreaseExpLostPercentageFormat, amount.ToString("N2"));
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
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null || owningCharacter.GameManager.ClientGuild == null)
                return;

            if (owningCharacter != null)
                owningCharacter.RequestAddGuildSkill(GuildSkill.DataId);
        }

        public void OnClickUse()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null || owningCharacter.GameManager.ClientGuild == null || owningCharacter.GameManager.ClientGuild.skillPoint <= 0)
                return;

            owningCharacter.RequestUseGuildSkill(GuildSkill.DataId);
        }
    }
}
