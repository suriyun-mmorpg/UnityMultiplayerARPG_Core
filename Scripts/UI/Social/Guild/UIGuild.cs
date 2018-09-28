using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UISocialCharacterSelectionManager))]
    public class UIGuild : UISocialGroup
    {
        [Header("Display Format")]
        [Tooltip("Guild Name Format => {0} = {guild name}")]
        public string guildNameFormat = "{0}";
        [Tooltip("Leader Name Format => {0} = {leader name}")]
        public string leaderNameFormat = "Leader: {0}";
        [Tooltip("Level Format => {0} = {Level}")]
        public string levelFormat = "Lv: {0}";
        [Tooltip("Exp Format => {0} = {Current exp}, {1} = {Max exp}")]
        public string expFormat = "Exp: {0}/{1}";
        [Tooltip("Skill Point Format => {0} = {Skill point}")]
        public string skillPointFormat = "Skill Points: {0}";
        [Tooltip("Message Format => {0} = {Message}")]
        public string messageFormat = "{0}";

        [Header("UI Elements")]
        public TextWrapper textGuildName;
        public TextWrapper textLeaderName;
        public TextWrapper textLevel;
        public TextWrapper textExp;
        public Image imageExpGage;
        public TextWrapper textSkillPoint;
        public TextWrapper textMessage;
        public InputFieldWrapper inputFieldMessage;
        public UIGuildCreate uiGuildCreate;
        public float refreshDuration = 1f;
        private float lastRefreshTime;
        
        public string guildName { get; private set; }
        public string leaderName { get; private set; }
        public int level { get; private set; }
        public int exp { get; private set; }
        public int skillPoint { get; private set; }
        public string guildMessage { get; private set; }

        private BaseGameNetworkManager cacheGameNetworkManager;
        public BaseGameNetworkManager CacheGameNetworkManager
        {
            get
            {
                if (cacheGameNetworkManager == null)
                    cacheGameNetworkManager = FindObjectOfType<BaseGameNetworkManager>();
                return cacheGameNetworkManager;
            }
        }

        protected override void Update()
        {
            base.Update();

            // Refresh guild info
            if (currentSocialId > 0)
            {
                if (Time.unscaledTime - lastRefreshTime >= refreshDuration)
                {
                    lastRefreshTime = Time.unscaledTime;
                    RefreshGuildInfo();
                }
            }
        }

        protected override void UpdateUIs()
        {
            if (textGuildName != null)
                textGuildName.text = string.Format(guildNameFormat, guildName);

            if (textLeaderName != null)
                textLeaderName.text = string.Format(leaderNameFormat, leaderName);

            if (textLevel != null)
                textLevel.text = string.Format(levelFormat, level.ToString("N0"));

            if (textExp != null)
                textExp.text = string.Format(expFormat, 0, 0);

            if (imageExpGage != null)
                imageExpGage.fillAmount = 1;

            if (textSkillPoint != null)
                textSkillPoint.text = string.Format(skillPointFormat, skillPoint.ToString("N0"));

            if (textMessage != null)
                textMessage.text = string.Format(messageFormat, guildMessage);

            base.UpdateUIs();
        }

        public void RefreshGuildInfo()
        {
            // Load cash shop item list
            CacheGameNetworkManager.RequestGuildData(ResponseGuildInfo);
        }

        public override void Show()
        {
            base.Show();
            RefreshGuildInfo();
        }

        public override void Hide()
        {
            if (uiGuildCreate != null)
                uiGuildCreate.Hide();
            base.Hide();
        }

        private void ResponseGuildInfo(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponseGuildDataMessage)message;
            if (responseCode == AckResponseCode.Success)
            {
                guildName = castedMessage.guildName;
                leaderName = castedMessage.leaderName;
                level = castedMessage.level;
                exp = castedMessage.exp;
                skillPoint = castedMessage.skillPoint;
                guildMessage = castedMessage.message;
                memberAmount = castedMessage.members.Length;
                UpdateUIs();

                var selectedIdx = SelectionManager.SelectedUI != null ? SelectionManager.IndexOf(SelectionManager.SelectedUI) : -1;
                SelectionManager.DeselectSelectedUI();
                SelectionManager.Clear();

                CacheList.Generate(castedMessage.members, (index, guildMember, ui) =>
                {
                    var guildMemberEntity = new SocialCharacterEntityTuple();
                    guildMemberEntity.socialCharacter = guildMember;

                    var uiGuildMember = ui.GetComponent<UISocialCharacter>();
                    uiGuildMember.uiSocialGroup = this;
                    uiGuildMember.Data = guildMemberEntity;
                    uiGuildMember.Show();
                    SelectionManager.Add(uiGuildMember);
                    if (selectedIdx == index)
                        uiGuildMember.OnClickSelect();
                });
            }
        }

        public void OnClickCreateGuild()
        {
            // If already in the guild, return
            if (currentSocialId > 0)
                return;
            // Show create guild dialog
            if (uiGuildCreate != null)
                uiGuildCreate.Show();
        }

        public void OnClickSetGuildMessage()
        {
            // If not in the guild or not leader, return
            if (!OwningCharacterIsLeader())
                return;

            // Show setup guild dialog
            if (inputFieldMessage != null)
                BasePlayerCharacterController.OwningCharacter.RequestSetGuildMessage(inputFieldMessage.text);
        }

        public void OnClickKickFromGuild()
        {
            // If not in the guild or not leader, return
            if (!OwningCharacterCanKick() || SelectionManager.SelectedUI == null)
                return;

            var guildMember = SelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog("Kick Member", string.Format("You sure you want to kick {0} from guild?", guildMember.characterName), false, true, false, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestKickFromGuild(guildMember.id);
            });
        }

        public void OnClickLeaveGuild()
        {
            UISceneGlobal.Singleton.ShowMessageDialog("Leave Guild", "You sure you want to leave guild?", false, true, false, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestLeaveGuild();
            });
        }

        public override int GetSocialId()
        {
            return BasePlayerCharacterController.OwningCharacter.GuildId;
        }

        public override int GetMaxMemberAmount()
        {
            return GameInstance.Singleton.SocialSystemSetting.maxGuildMember;
        }

        public override bool IsLeader(byte flags)
        {
            return GuildData.IsLeader((GuildMemberFlags)flags);
        }

        public override bool CanKick(byte flags)
        {
            return GuildData.CanKick((GuildMemberFlags)flags);
        }

        public override bool OwningCharacterIsLeader()
        {
            return GuildData.IsLeader(BasePlayerCharacterController.OwningCharacter.GuildMemberFlags);
        }

        public override bool OwningCharacterCanKick()
        {
            return GuildData.CanKick(BasePlayerCharacterController.OwningCharacter.GuildMemberFlags);
        }
    }
}
