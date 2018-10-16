using UnityEngine;
using UnityEngine.UI;

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

        public GuildData Guild { get { return CacheGameNetworkManager.ClientGuild; } }

        private string guildMessage;

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

        protected override void UpdateUIs()
        {
            if (textGuildName != null)
                textGuildName.text = string.Format(guildNameFormat, Guild == null ? "Unknow" : Guild.guildName);

            if (textLeaderName != null)
                textLeaderName.text = string.Format(leaderNameFormat, Guild == null ? "Unknow" : Guild.GetLeader().characterName);

            if (textLevel != null)
                textLevel.text = string.Format(levelFormat, Guild == null ? "1" : Guild.level.ToString("N0"));

            var expTree = GameInstance.Singleton.SocialSystemSetting.GuildExpTree;
            var currentExp = 0;
            var nextLevelExp = 0;
            if (Guild != null && Guild.GetNextLevelExp() > 0)
            {
                currentExp = Guild.exp;
                nextLevelExp = Guild.GetNextLevelExp();
            }
            else if (Guild != null && Guild.level - 2 > 0 && Guild.level - 2 < expTree.Length)
            {
                var maxExp = expTree[Guild.level - 2];
                currentExp = maxExp;
                nextLevelExp = maxExp;
            }

            if (textExp != null)
                textExp.text = string.Format(expFormat, currentExp.ToString("N0"), nextLevelExp.ToString("N0"));

            if (imageExpGage != null)
                imageExpGage.fillAmount = nextLevelExp <= 0 ? 1 : (float)currentExp / (float)nextLevelExp;

            if (textSkillPoint != null)
                textSkillPoint.text = string.Format(skillPointFormat, Guild == null ? "0" : Guild.skillPoint.ToString("N0"));

            if (Guild == null)
            {
                if (textMessage != null)
                    textMessage.text = string.Format(messageFormat, string.Empty);

                if (inputFieldMessage != null)
                    inputFieldMessage.text = string.Empty;
            }

            if (Guild != null && !Guild.guildMessage.Equals(guildMessage))
            {
                guildMessage = Guild.guildMessage;

                if (textMessage != null)
                    textMessage.text = string.Format(messageFormat, guildMessage);

                if (inputFieldMessage != null)
                    inputFieldMessage.text = guildMessage;
            }

            base.UpdateUIs();
        }

        private void OnEnable()
        {
            CacheGameNetworkManager.onClientUpdateGuild += UpdateGuildUIs;
        }

        private void OnDisable()
        {
            CacheGameNetworkManager.onClientUpdateGuild -= UpdateGuildUIs;
        }

        public override void Show()
        {
            base.Show();
            RoleSelectionManager.eventOnSelect.RemoveListener(OnSelectRole);
            RoleSelectionManager.eventOnSelect.AddListener(OnSelectRole);
            RoleSelectionManager.eventOnDeselect.RemoveListener(OnDeselectRole);
            RoleSelectionManager.eventOnDeselect.AddListener(OnDeselectRole);
            UpdateGuildUIs(Guild);
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

        private void UpdateGuildUIs(GuildData guild)
        {
            if (guild == null)
                return;

            memberAmount = guild.CountMember();
            UpdateUIs();

            var selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            SocialCharacterData[] members;
            byte[] memberRoles;
            guild.GetSortedMembers(out members, out memberRoles);
            MemberList.Generate(members, (index, guildMember, ui) =>
            {
                var guildMemberEntity = new SocialCharacterEntityTuple();
                guildMemberEntity.socialCharacter = guildMember;

                var uiGuildMember = ui.GetComponent<UIGuildCharacter>();
                uiGuildMember.uiSocialGroup = this;
                uiGuildMember.Setup(guildMemberEntity, memberRoles[index], guild.GetRole(memberRoles[index]));
                uiGuildMember.Show();
                MemberSelectionManager.Add(uiGuildMember);
                if (selectedIdx == index)
                    uiGuildMember.OnClickSelect();
            });

            selectedIdx = RoleSelectionManager.SelectedUI != null ? RoleSelectionManager.IndexOf(RoleSelectionManager.SelectedUI) : -1;
            RoleSelectionManager.DeselectSelectedUI();
            RoleSelectionManager.Clear();

            RoleList.Generate(guild.GetRoles(), (index, guildRole, ui) =>
            {
                var uiGuildRole = ui.GetComponent<UIGuildRole>();
                uiGuildRole.Data = guildRole;
                uiGuildRole.Show();
                RoleSelectionManager.Add(uiGuildRole);
                if (selectedIdx == index)
                    uiGuildRole.OnClickSelect();
            });
        }

        public bool IsRoleAvailable(byte guildRole)
        {
            return Guild != null ? Guild.IsRoleAvailable(guildRole) : false;
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
            if (!OwningCharacterIsLeader() || MemberSelectionManager.SelectedUI == null)
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
            if (!OwningCharacterIsLeader() || Guild == null || RoleSelectionManager.SelectedUI == null)
                return;

            if (uiGuildRoleSetting != null)
            {
                var guildRole = (byte)RoleSelectionManager.IndexOf(RoleSelectionManager.SelectedUI);
                var role = Guild.GetRole(guildRole);
                uiGuildRoleSetting.Show(guildRole, role.roleName, role.canInvite, role.canKick, role.shareExpPercentage);
            }
        }

        public void OnClickSetMemberRole()
        {
            // If not in the guild or not leader, return
            if (!OwningCharacterIsLeader() || Guild == null || MemberSelectionManager.SelectedUI == null)
                return;

            var selectedUI = MemberSelectionManager.SelectedUI as UIGuildCharacter;
            if (uiGuildMemberRoleSetting != null && selectedUI != null)
                uiGuildMemberRoleSetting.Show(Guild.GetRoles().ToArray(), selectedUI.Data.socialCharacter, selectedUI.GuildRole);
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

        public override bool IsLeader(string characterId)
        {
            return CacheGameNetworkManager.ClientGuild != null && CacheGameNetworkManager.ClientGuild.IsLeader(characterId);
        }

        public override bool IsOnline(string characterId)
        {
            return CacheGameNetworkManager.ClientGuild != null && CacheGameNetworkManager.ClientGuild.IsOnline(characterId);
        }

        public override bool CanKick(string characterId)
        {
            return CacheGameNetworkManager.ClientGuild != null && CacheGameNetworkManager.ClientGuild.CanKick(characterId);
        }

        public override bool OwningCharacterIsLeader()
        {
            return IsLeader(BasePlayerCharacterController.OwningCharacter.Id);
        }

        public override bool OwningCharacterCanKick()
        {
            return CanKick(BasePlayerCharacterController.OwningCharacter.Id);
        }
    }
}
