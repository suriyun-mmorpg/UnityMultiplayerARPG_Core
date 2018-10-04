using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UISocialCharacterSelectionManager))]
    [RequireComponent(typeof(UIGuildRoleSelectionManager))]
    public class UIGuild : UISocialGroup<UIGuildCharacter>
    {
        [Header("Display Format")]
        [Tooltip("Guild Name Format => {0} = {Guild name}")]
        public string guildNameFormat = "{0}";
        [Tooltip("Leader Name Format => {0} = {Leader name}")]
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
        public UIGuildRole uiRoleDialog;
        public UIGuildRole uiRolePrefab;
        public Transform uiRoleContainer;
        public TextWrapper textGuildName;
        public TextWrapper textLeaderName;
        public TextWrapper textLevel;
        public TextWrapper textExp;
        public Image imageExpGage;
        public TextWrapper textSkillPoint;
        public TextWrapper textMessage;
        public InputFieldWrapper inputFieldMessage;
        public UIGuildCreate uiGuildCreate;
        public UIGuildRoleSetting uiGuildRoleSetting;
        public UIGuildMemberRoleSetting uiGuildMemberRoleSetting;
        public float refreshDuration = 1f;
        private float lastRefreshTime;
        
        public string guildName { get; private set; }
        public string leaderName { get; private set; }
        public int level { get; private set; }
        public int exp { get; private set; }
        public int skillPoint { get; private set; }
        public string guildMessage { get; private set; }
        public GuildRole[] roles { get; private set; }
        public byte[] memberRoles { get; private set; }

        private UIList roleList;
        public UIList RoleList
        {
            get
            {
                if (roleList == null)
                {
                    roleList = gameObject.AddComponent<UIList>();
                    roleList.uiPrefab = uiRolePrefab.gameObject;
                    roleList.uiContainer = uiRoleContainer;
                }
                return roleList;
            }
        }

        private UIGuildRoleSelectionManager roleSelectionManager;
        public UIGuildRoleSelectionManager RoleSelectionManager
        {
            get
            {
                if (roleSelectionManager == null)
                    roleSelectionManager = GetComponent<UIGuildRoleSelectionManager>();
                roleSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return roleSelectionManager;
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
            RoleSelectionManager.eventOnSelect.RemoveListener(OnSelectRole);
            RoleSelectionManager.eventOnSelect.AddListener(OnSelectRole);
            RoleSelectionManager.eventOnDeselect.RemoveListener(OnDeselectRole);
            RoleSelectionManager.eventOnDeselect.AddListener(OnDeselectRole);
            RefreshGuildInfo();
        }

        public override void Hide()
        {
            if (uiGuildCreate != null)
                uiGuildCreate.Hide();
            RoleSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectRole(UIGuildRole ui)
        {
            if (uiRoleDialog != null)
            {
                uiRoleDialog.selectionManager = RoleSelectionManager;
                uiRoleDialog.Data = ui.Data;
                uiRoleDialog.Show();
            }
        }

        protected void OnDeselectRole(UIGuildRole ui)
        {
            if (uiRoleDialog != null)
                uiRoleDialog.Hide();
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
                roles = castedMessage.roles;
                memberRoles = castedMessage.memberRoles;
                memberAmount = castedMessage.members.Length;
                UpdateUIs();

                var selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
                MemberSelectionManager.DeselectSelectedUI();
                MemberSelectionManager.Clear();

                MemberList.Generate(castedMessage.members, (index, guildMember, ui) =>
                {
                    var guildMemberEntity = new SocialCharacterEntityTuple();
                    guildMemberEntity.socialCharacter = guildMember;

                    var uiGuildMember = ui.GetComponent<UIGuildCharacter>();
                    uiGuildMember.uiSocialGroup = this;
                    uiGuildMember.Setup(guildMemberEntity, memberRoles[index], GetGuildRole(memberRoles[index]));
                    uiGuildMember.Show();
                    MemberSelectionManager.Add(uiGuildMember);
                    if (selectedIdx == index)
                        uiGuildMember.OnClickSelect();
                });

                selectedIdx = RoleSelectionManager.SelectedUI != null ? RoleSelectionManager.IndexOf(RoleSelectionManager.SelectedUI) : -1;
                RoleSelectionManager.DeselectSelectedUI();
                RoleSelectionManager.Clear();

                RoleList.Generate(castedMessage.roles, (index, guildRole, ui) =>
                {
                    var uiGuildRole = ui.GetComponent<UIGuildRole>();
                    uiGuildRole.Data = guildRole;
                    uiGuildRole.Show();
                    RoleSelectionManager.Add(uiGuildRole);
                    if (selectedIdx == index)
                        uiGuildRole.OnClickSelect();
                });
            }
        }

        public GuildRole GetGuildRole(byte guildRole)
        {
            if (!IsRoleAvailable(guildRole))
            {
                if (guildRole == GuildData.LeaderRole)
                    return new GuildRole() { roleName = "Master", canInvite = true, canKick = true };
                else
                    return new GuildRole() { roleName = "Member", canInvite = false, canKick = false };
            }
            return roles[guildRole];
        }

        public bool IsRoleAvailable(byte guildRole)
        {
            return roles != null && guildRole >= 0 && guildRole < roles.Length;
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

        public void OnClickChangeLeader()
        {
            // If not in the guild or not leader, return
            if (!OwningCharacterIsLeader())
                return;

            var guildMember = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog("Change Leader", string.Format("You sure you want to promote {0} to guild leader?", guildMember.characterName), false, true, false, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestChangeGuildLeader(guildMember.id);
            });
        }

        public void OnClickSetRole()
        {
            // If not in the guild or not leader, return
            if (!OwningCharacterIsLeader() || RoleSelectionManager.SelectedUI == null)
                return;

            if (uiGuildRoleSetting != null)
            {
                var guildRole = (byte)RoleSelectionManager.IndexOf(RoleSelectionManager.SelectedUI);
                var role = GetGuildRole(guildRole);
                uiGuildRoleSetting.Show(guildRole, role.roleName, role.canInvite, role.canKick, role.shareExpPercentage);
            }
        }

        public void OnClickSetMemberRole()
        {
            // If not in the guild or not leader, return
            if (!OwningCharacterIsLeader() || MemberSelectionManager.SelectedUI == null)
                return;

            var selectedUI = MemberSelectionManager.SelectedUI as UIGuildCharacter;
            if (uiGuildMemberRoleSetting != null && selectedUI != null)
                uiGuildMemberRoleSetting.Show(roles, selectedUI.Data.socialCharacter, selectedUI.GuildRole);
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
            if (!OwningCharacterCanKick() || MemberSelectionManager.SelectedUI == null)
                return;

            var guildMember = MemberSelectionManager.SelectedUI.Data.socialCharacter;
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
            return GameInstance.Singleton.SocialSystemSetting.MaxGuildMember;
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
