using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class UIGuild : UISocialGroup<UIGuildCharacter>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Guild Name}")]
        public UILocaleKeySetting formatKeyGuildName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Leader Name}")]
        public UILocaleKeySetting formatKeyLeaderName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SOCIAL_LEADER);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {Skill Point}")]
        public UILocaleKeySetting formatKeySkillPoint = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_POINTS);
        [Tooltip("Format => {0} = {Message}")]
        public UILocaleKeySetting formatKeyMessage = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SOCIAL_LEADER);

        [Header("UI Elements")]
        public UIGuildRole uiRoleDialog;
        public UIGuildRole uiRolePrefab;
        public Transform uiRoleContainer;
        public UIGuildSkill uiSkillDialog;
        public UIGuildSkill uiSkillPrefab;
        public Transform uiSkillContainer;
        public TextWrapper textGuildName;
        public TextWrapper textLeaderName;
        public TextWrapper textLevel;
        public UIGageValue uiGageExp;

        public TextWrapper textSkillPoint;
        public TextWrapper textMessage;
        public InputFieldWrapper inputFieldMessage;
        public UIGuildCreate uiGuildCreate;
        public UIGuildRoleSetting uiGuildRoleSetting;
        public UIGuildMemberRoleSetting uiGuildMemberRoleSetting;

        public GuildData Guild { get { return BaseGameNetworkManager.ClientGuild; } }

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
                if (roleSelectionManager == null)
                    roleSelectionManager = gameObject.AddComponent<UIGuildRoleSelectionManager>();
                roleSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return roleSelectionManager;
            }
        }

        private UIList skillList;
        public UIList SkillList
        {
            get
            {
                if (skillList == null)
                {
                    skillList = gameObject.AddComponent<UIList>();
                    skillList.uiPrefab = uiSkillPrefab.gameObject;
                    skillList.uiContainer = uiSkillContainer;
                }
                return skillList;
            }
        }

        private UIGuildSkillSelectionManager skillSelectionManager;
        public UIGuildSkillSelectionManager SkillSelectionManager
        {
            get
            {
                if (skillSelectionManager == null)
                    skillSelectionManager = GetComponent<UIGuildSkillSelectionManager>();
                if (skillSelectionManager == null)
                    skillSelectionManager = gameObject.AddComponent<UIGuildSkillSelectionManager>();
                skillSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return skillSelectionManager;
            }
        }

        protected override void UpdateUIs()
        {
            if (textGuildName != null)
            {
                textGuildName.text = string.Format(
                    LanguageManager.GetText(formatKeyGuildName),
                    Guild == null ? LanguageManager.GetUnknowTitle() : Guild.guildName);
            }

            if (textLeaderName != null)
            {
                textLeaderName.text = string.Format(
                    LanguageManager.GetText(formatKeyLeaderName),
                    Guild == null ? LanguageManager.GetUnknowTitle() : Guild.GetLeader().characterName);
            }

            if (textLevel != null)
            {
                textLevel.text = string.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Guild == null ? "0" : Guild.level.ToString("N0"));
            }

            int[] expTree = GameInstance.Singleton.SocialSystemSetting.GuildExpTree;
            int currentExp = 0;
            int nextLevelExp = 0;
            if (Guild != null && Guild.GetNextLevelExp() > 0)
            {
                currentExp = Guild.exp;
                nextLevelExp = Guild.GetNextLevelExp();
            }
            else if (Guild != null && Guild.level - 2 > 0 && Guild.level - 2 < expTree.Length)
            {
                int maxExp = expTree[Guild.level - 2];
                currentExp = maxExp;
                nextLevelExp = maxExp;
            }
            if (uiGageExp != null)
                uiGageExp.Update(currentExp, nextLevelExp);

            if (textSkillPoint != null)
            {
                textSkillPoint.text = string.Format(
                    LanguageManager.GetText(formatKeySkillPoint),
                    Guild == null ? "0" : Guild.skillPoint.ToString("N0"));
            }

            if (Guild == null)
            {
                if (textMessage != null)
                    textMessage.text = string.Format(LanguageManager.GetText(formatKeyMessage), string.Empty);

                if (inputFieldMessage != null)
                    inputFieldMessage.text = string.Empty;
            }

            if (Guild != null && !Guild.guildMessage.Equals(guildMessage))
            {
                guildMessage = Guild.guildMessage;

                if (textMessage != null)
                    textMessage.text = string.Format(LanguageManager.GetText(formatKeyMessage), guildMessage);

                if (inputFieldMessage != null)
                    inputFieldMessage.text = guildMessage;
            }

            base.UpdateUIs();
        }

        private void OnEnable()
        {
            BaseGameNetworkManager.Singleton.onClientUpdateGuild += UpdateGuildUIs;
        }

        private void OnDisable()
        {
            BaseGameNetworkManager.Singleton.onClientUpdateGuild -= UpdateGuildUIs;
        }

        public override void Show()
        {
            base.Show();
            RoleSelectionManager.eventOnSelect.RemoveListener(OnSelectRole);
            RoleSelectionManager.eventOnSelect.AddListener(OnSelectRole);
            RoleSelectionManager.eventOnDeselect.RemoveListener(OnDeselectRole);
            RoleSelectionManager.eventOnDeselect.AddListener(OnDeselectRole);
            SkillSelectionManager.eventOnSelect.RemoveListener(OnSelectSkill);
            SkillSelectionManager.eventOnSelect.AddListener(OnSelectSkill);
            SkillSelectionManager.eventOnDeselect.RemoveListener(OnDeselectSkill);
            SkillSelectionManager.eventOnDeselect.AddListener(OnDeselectSkill);
            if (uiRoleDialog != null)
                uiRoleDialog.onHide.AddListener(OnRoleDialogHide);
            if (uiSkillDialog != null)
                uiSkillDialog.onHide.AddListener(OnSkillDialogHide);
            UpdateGuildUIs(Guild);
        }

        public override void Hide()
        {
            if (uiGuildCreate != null)
                uiGuildCreate.Hide();
            if (uiRoleDialog != null)
                uiRoleDialog.onHide.RemoveListener(OnRoleDialogHide);
            if (uiSkillDialog != null)
                uiSkillDialog.onHide.RemoveListener(OnSkillDialogHide);
            RoleSelectionManager.DeselectSelectedUI();
            SkillSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnRoleDialogHide()
        {
            RoleSelectionManager.DeselectSelectedUI();
        }

        protected void OnSkillDialogHide()
        {
            SkillSelectionManager.DeselectSelectedUI();
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
            {
                uiRoleDialog.onHide.RemoveListener(OnRoleDialogHide);
                uiRoleDialog.Hide();
                uiRoleDialog.onHide.AddListener(OnRoleDialogHide);
            }
        }

        protected void OnSelectSkill(UIGuildSkill ui)
        {
            if (uiSkillDialog != null)
            {
                uiSkillDialog.selectionManager = SkillSelectionManager;
                uiSkillDialog.Data = ui.Data;
                uiSkillDialog.Show();
            }
        }

        protected void OnDeselectSkill(UIGuildSkill ui)
        {
            if (uiSkillDialog != null)
            {
                uiSkillDialog.onHide.RemoveListener(OnSkillDialogHide);
                uiSkillDialog.Hide();
                uiSkillDialog.onHide.AddListener(OnSkillDialogHide);
            }
        }

        private void UpdateGuildUIs(GuildData guild)
        {
            if (guild == null)
                return;

            memberAmount = guild.CountMember();
            UpdateUIs();

            int selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            SocialCharacterData[] members;
            byte[] memberRoles;
            guild.GetSortedMembers(out members, out memberRoles);
            MemberList.Generate(members, (index, guildMember, ui) =>
            {
                UISocialCharacterData guildMemberEntity = new UISocialCharacterData();
                guildMemberEntity.socialCharacter = guildMember;

                UIGuildCharacter uiGuildMember = ui.GetComponent<UIGuildCharacter>();
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
                UIGuildRole uiGuildRole = ui.GetComponent<UIGuildRole>();
                uiGuildRole.Data = guildRole;
                uiGuildRole.Show();
                RoleSelectionManager.Add(uiGuildRole);
                if (selectedIdx == index)
                    uiGuildRole.OnClickSelect();
            });

            selectedIdx = SkillSelectionManager.SelectedUI != null ? SkillSelectionManager.IndexOf(SkillSelectionManager.SelectedUI) : -1;
            SkillSelectionManager.DeselectSelectedUI();
            SkillSelectionManager.Clear();

            SkillList.Generate(GameInstance.GuildSkills.Values, (index, guildSkill, ui) =>
            {
                UIGuildSkill uiGuildSkill = ui.GetComponent<UIGuildSkill>();
                uiGuildSkill.Data = new UIGuildSkillData(guildSkill, guild.GetSkillLevel(guildSkill.DataId));
                uiGuildSkill.Show();
                SkillSelectionManager.Add(uiGuildSkill);
                if (selectedIdx == index)
                    uiGuildSkill.OnClickSelect();
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

            SocialCharacterData guildMember = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_GUILD_CHANGE_LEADER.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_GUILD_CHANGE_LEADER_DESCRIPTION.ToString()), guildMember.characterName), false, true, true, false, null, () =>
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
                byte guildRole = (byte)RoleSelectionManager.IndexOf(RoleSelectionManager.SelectedUI);
                GuildRoleData role = Guild.GetRole(guildRole);
                uiGuildRoleSetting.Show(guildRole, role.roleName, role.canInvite, role.canKick, role.shareExpPercentage);
            }
        }

        public void OnClickSetMemberRole()
        {
            // If not in the guild or not leader, return
            if (!OwningCharacterIsLeader() || Guild == null || MemberSelectionManager.SelectedUI == null)
                return;

            UIGuildCharacter selectedUI = MemberSelectionManager.SelectedUI as UIGuildCharacter;
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

            SocialCharacterData guildMember = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_GUILD_KICK_MEMBER.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_GUILD_KICK_MEMBER_DESCRIPTION.ToString()), guildMember.characterName), false, true, true, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestKickFromGuild(guildMember.id);
            });
        }

        public void OnClickLeaveGuild()
        {
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_GUILD_LEAVE.ToString()), LanguageManager.GetText(UITextKeys.UI_GUILD_LEAVE_DESCRIPTION.ToString()), false, true, true, false, null, () =>
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
            if (Guild == null)
                return 0;
            return Guild.MaxMember();
        }

        public override bool IsLeader(string characterId)
        {
            return Guild != null && Guild.IsLeader(characterId);
        }

        public override bool CanKick(string characterId)
        {
            return Guild != null && Guild.CanKick(characterId);
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
