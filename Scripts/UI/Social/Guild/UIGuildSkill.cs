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
                owningCharacter.GameManager != null &&
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
            if (owningCharacter == null || owningCharacter.GuildId <= 0)
                return;

            if (owningCharacter != null)
                owningCharacter.RequestAddGuildSkill(GuildSkill.DataId);
        }

        public void OnClickUse()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null || owningCharacter.GuildId <= 0)
                return;

            owningCharacter.RequestUseGuildSkill(GuildSkill.DataId);
        }
    }
}
