using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIGuildSkill : UISelectionEntry<GuildSkillTuple>
    {
        public GuildSkill guildSkill { get { return Data.guildSkill; } }
        public short level { get { return Data.targetLevel; } }

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

        [Header("Options")]
        public UIGuildSkill uiNextLevelSkill;

        protected override void UpdateData()
        {
            if (level <= 0)
                onSetLevelZeroData.Invoke();
            else
                onSetNonLevelZeroData.Invoke();

            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(titleFormat, guildSkill == null ? "Unknow" : guildSkill.title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, guildSkill == null ? "N/A" : guildSkill.description);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, level.ToString("N0"));

            if (imageIcon != null)
            {
                var iconSprite = guildSkill == null ? null : guildSkill.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiTextSkillType != null)
            {
                switch (guildSkill.skillType)
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
                if (!guildSkill.IsBuff())
                    uiSkillBuff.Hide();
                else
                {
                    uiSkillBuff.Show();
                    uiSkillBuff.Data = new BuffTuple(guildSkill.buff, level);
                }
            }

            if (uiNextLevelSkill != null)
            {
                if (level + 1 > guildSkill.maxLevel)
                    uiNextLevelSkill.Hide();
                else
                {
                    uiNextLevelSkill.Data = new GuildSkillTuple(guildSkill, (short)(level + 1));
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
                owningCharacter.RequestAddGuildSkill(guildSkill.DataId);
        }
    }
}
