using Cysharp.Text;
using LiteNetLibManager;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

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
        public UILocaleKeySetting formatKeyMessage = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Message2}")]
        public UILocaleKeySetting formatKeyMessage2 = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Score}")]
        public UILocaleKeySetting formatKeyScore = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Rank}")]
        public UILocaleKeySetting formatKeyRank = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public UIGuildCreate uiGuildCreate;
        public UIGuildRoleSetting uiGuildRoleSetting;
        public UIGuildMemberRoleSetting uiGuildMemberRoleSetting;
        public UIGuildRole uiRoleDialog;
        public UIGuildRole uiRolePrefab;
        public Transform uiRoleContainer;
        public UIGuildSkill uiSkillDialog;
        public UIGuildSkill uiSkillPrefab;
        public Transform uiSkillContainer;
        public UIGuildIcon uiGuildIcon;
        public TextWrapper textGuildName;
        public TextWrapper textLeaderName;
        public TextWrapper textLevel;
        public UIGageValue uiGageExp;
        public TextWrapper textSkillPoint;
        public TextWrapper textMessage;
        public InputFieldWrapper inputFieldMessage;
        public TextWrapper textMessage2;
        public InputFieldWrapper inputFieldMessage2;
        public Toggle toggleAutoAcceptRequests;
        public Toggle toggleNotAutoAcceptRequests;
        public TextWrapper textScore;
        public TextWrapper textRank;
        public GameObject[] autoAcceptRequestsObjects;
        public GameObject[] notAutoAcceptRequestsObjects;

        public GuildData Guild { get { return GameInstance.JoinedGuild; } }

        private string guildMessage;
        private string guildMessage2;

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
                    roleSelectionManager = gameObject.GetOrAddComponent<UIGuildRoleSelectionManager>();
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
                    skillSelectionManager = gameObject.GetOrAddComponent<UIGuildSkillSelectionManager>();
                skillSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return skillSelectionManager;
            }
        }

        private UIGuildMessageUpdater guildMessageUpdater;
        public UIGuildMessageUpdater GuildMessageUpdater
        {
            get
            {
                if (guildMessageUpdater == null)
                    guildMessageUpdater = gameObject.GetOrAddComponent<UIGuildMessageUpdater>();
                guildMessageUpdater.inputField = inputFieldMessage;
                return guildMessageUpdater;
            }
        }

        private UIGuildMessage2Updater guildMessage2Updater;
        public UIGuildMessage2Updater GuildMessage2Updater
        {
            get
            {
                if (guildMessage2Updater == null)
                    guildMessage2Updater = gameObject.GetOrAddComponent<UIGuildMessage2Updater>();
                guildMessage2Updater.inputField = inputFieldMessage2;
                return guildMessage2Updater;
            }
        }

        private UIGuildAutoAcceptRequestUpdater guildAutoAcceptRequestUpdater;
        public UIGuildAutoAcceptRequestUpdater GuildAutoAcceptRequestUpdater
        {
            get
            {
                if (guildAutoAcceptRequestUpdater == null)
                    guildAutoAcceptRequestUpdater = gameObject.GetOrAddComponent<UIGuildAutoAcceptRequestUpdater>();
                guildAutoAcceptRequestUpdater.toggle = toggleAutoAcceptRequests;
                return guildAutoAcceptRequestUpdater;
            }
        }

        private UIGuildIconUpdater guildIconUpdater;
        public UIGuildIconUpdater GuildIconUpdater
        {
            get
            {
                if (guildIconUpdater == null)
                    guildIconUpdater = gameObject.GetOrAddComponent<UIGuildIconUpdater>();
                guildIconUpdater.uiGuildIcon = uiGuildIcon;
                return guildIconUpdater;
            }
        }

        protected override void UpdateUIs()
        {
            GuildOptions options = new GuildOptions();
            if (Guild != null && !string.IsNullOrEmpty(Guild.options))
                options = JsonConvert.DeserializeObject<GuildOptions>(Guild.options);

            if (uiGuildIcon != null)
                uiGuildIcon.SetDataByDataId(options.iconDataId);

            if (textGuildName != null)
            {
                textGuildName.text = ZString.Format(
                    LanguageManager.GetText(formatKeyGuildName),
                    Guild == null ? LanguageManager.GetUnknowTitle() : Guild.guildName);
            }

            if (textLeaderName != null)
            {
                textLeaderName.text = ZString.Format(
                    LanguageManager.GetText(formatKeyLeaderName),
                    Guild == null ? LanguageManager.GetUnknowTitle() : Guild.GetLeader().characterName);
            }

            if (textLevel != null)
            {
                textLevel.text = ZString.Format(
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
                textSkillPoint.text = ZString.Format(
                    LanguageManager.GetText(formatKeySkillPoint),
                    Guild == null ? "0" : Guild.skillPoint.ToString("N0"));
            }

            if (Guild == null)
            {
                if (textMessage != null)
                    textMessage.text = ZString.Format(LanguageManager.GetText(formatKeyMessage), string.Empty);

                if (inputFieldMessage != null)
                    inputFieldMessage.text = string.Empty;
            }

            if (Guild != null && Guild.guildMessage != null && !Guild.guildMessage.Equals(guildMessage))
            {
                guildMessage = Guild.guildMessage;

                if (textMessage != null)
                    textMessage.text = ZString.Format(LanguageManager.GetText(formatKeyMessage), guildMessage);

                if (inputFieldMessage != null)
                    inputFieldMessage.text = guildMessage;
            }

            if (Guild == null)
            {
                if (textMessage2 != null)
                    textMessage2.text = ZString.Format(LanguageManager.GetText(formatKeyMessage2), string.Empty);

                if (inputFieldMessage2 != null)
                    inputFieldMessage2.text = string.Empty;
            }

            if (Guild != null && Guild.guildMessage2 != null && !Guild.guildMessage2.Equals(guildMessage2))
            {
                guildMessage2 = Guild.guildMessage2;

                if (textMessage2 != null)
                    textMessage2.text = ZString.Format(LanguageManager.GetText(formatKeyMessage2), guildMessage2);

                if (inputFieldMessage2 != null)
                    inputFieldMessage2.text = guildMessage2;
            }

            if (toggleAutoAcceptRequests != null)
                toggleAutoAcceptRequests.isOn = Guild != null && Guild.autoAcceptRequests;

            if (toggleNotAutoAcceptRequests != null)
                toggleNotAutoAcceptRequests.isOn = Guild == null || !Guild.autoAcceptRequests;

            if (textScore != null)
            {
                textScore.text = ZString.Format(
                    LanguageManager.GetText(formatKeyScore), 
                    Guild == null ? "0" : Guild.score.ToString("N0"));
            }

            if (textRank != null)
            {
                textRank.text = ZString.Format(
                    LanguageManager.GetText(formatKeyRank),
                    Guild == null || Guild.rank == 0 ? "N/A" : Guild.rank.ToString("N0"));
            }

            if (autoAcceptRequestsObjects != null && autoAcceptRequestsObjects.Length > 0)
            {
                foreach (GameObject autoAcceptRequestsObject in autoAcceptRequestsObjects)
                {
                    autoAcceptRequestsObject.SetActive(Guild != null && Guild.autoAcceptRequests);
                }
            }

            if (notAutoAcceptRequestsObjects != null && notAutoAcceptRequestsObjects.Length > 0)
            {
                foreach (GameObject notAutoAcceptRequestsObject in notAutoAcceptRequestsObjects)
                {
                    notAutoAcceptRequestsObject.SetActive(Guild == null || !Guild.autoAcceptRequests);
                }
            }

            base.UpdateUIs();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
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
            UpdateGuildUIs(UpdateGuildMessage.UpdateType.Member, Guild);
            ClientGuildActions.onNotifyGuildUpdated += UpdateGuildUIs;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (uiGuildCreate != null)
                uiGuildCreate.Hide();
            if (uiRoleDialog != null)
                uiRoleDialog.onHide.RemoveListener(OnRoleDialogHide);
            if (uiSkillDialog != null)
                uiSkillDialog.onHide.RemoveListener(OnSkillDialogHide);
            RoleSelectionManager.DeselectSelectedUI();
            SkillSelectionManager.DeselectSelectedUI();
            ClientGuildActions.onNotifyGuildUpdated -= UpdateGuildUIs;
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

        private void UpdateGuildUIs(UpdateGuildMessage.UpdateType updateType, GuildData guild)
        {
            if (guild == null)
                return;

            memberAmount = guild.CountMember();
            UpdateUIs();

            // Members
            string selectedId = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.SelectedUI.Data.id : string.Empty;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            SocialCharacterData[] members;
            byte[] memberRoles;
            guild.GetSortedMembers(out members, out memberRoles);
            MemberList.Generate(members, (index, data, ui) =>
            {
                UIGuildCharacter uiGuildMember = ui.GetComponent<UIGuildCharacter>();
                uiGuildMember.uiSocialGroup = this;
                uiGuildMember.Setup(data, memberRoles[index], guild.GetRole(memberRoles[index]));
                uiGuildMember.Show();
                MemberSelectionManager.Add(uiGuildMember);
                if (selectedId.Equals(data.id))
                    uiGuildMember.OnClickSelect();
            });

            // Roles
            int selectedIdx = RoleSelectionManager.SelectedUI != null ? RoleSelectionManager.IndexOf(RoleSelectionManager.SelectedUI) : -1;
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

            // Skills
            int selectedDataId = SkillSelectionManager.SelectedUI != null ? SkillSelectionManager.SelectedUI.Data.guildSkill.DataId : 0;
            SkillSelectionManager.DeselectSelectedUI();
            SkillSelectionManager.Clear();

            SkillList.Generate(GameInstance.GuildSkills.Values, (index, guildSkill, ui) =>
            {
                UIGuildSkill uiGuildSkill = ui.GetComponent<UIGuildSkill>();
                uiGuildSkill.Data = new UIGuildSkillData(guildSkill, guild.GetSkillLevel(guildSkill.DataId));
                uiGuildSkill.Show();
                SkillSelectionManager.Add(uiGuildSkill);
                if (selectedDataId == guildSkill.DataId)
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

            SocialCharacterData guildMember = MemberSelectionManager.SelectedUI.Data;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_GUILD_CHANGE_LEADER.ToString()), ZString.Format(LanguageManager.GetText(UITextKeys.UI_GUILD_CHANGE_LEADER_DESCRIPTION.ToString()), guildMember.characterName), false, true, true, false, null, () =>
            {
                GameInstance.ClientGuildHandlers.RequestChangeGuildLeader(new RequestChangeGuildLeaderMessage()
                {
                    memberId = guildMember.id,
                }, ChangeGuildLeaderCallback);
            });
        }

        private void ChangeGuildLeaderCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildLeaderMessage response)
        {
            ClientGuildActions.ResponseChangeGuildLeader(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
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
                uiGuildRoleSetting.Show(guildRole, role);
            }
        }

        public void OnClickSetMemberRole()
        {
            // If not in the guild or not leader, return
            if (!OwningCharacterIsLeader() || Guild == null || MemberSelectionManager.SelectedUI == null)
                return;

            UIGuildCharacter selectedUI = MemberSelectionManager.SelectedUI as UIGuildCharacter;
            if (uiGuildMemberRoleSetting != null && selectedUI != null)
                uiGuildMemberRoleSetting.Show(Guild.GetRoles().ToArray(), selectedUI.Data, selectedUI.GuildRole);
        }

        public void OnClickSetGuildMessage()
        {
            // If not in the guild or not leader, return
            if (!OwningCharacterIsLeader())
                return;
            GuildMessageUpdater.UpdateData();
        }

        public void OnClickSetGuildMessage2()
        {
            // If not in the guild or not leader, return
            if (!OwningCharacterIsLeader())
                return;
            GuildMessage2Updater.UpdateData();
        }

        public void OnClickSetGuildIcon()
        {
            // If not in the guild or not leader, return
            if (!OwningCharacterIsLeader())
                return;
            GuildIconUpdater.UpdateData();
        }

        public void OnClickLeaveGuild()
        {
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_GUILD_LEAVE.ToString()), LanguageManager.GetText(UITextKeys.UI_GUILD_LEAVE_DESCRIPTION.ToString()), false, true, true, false, null, () =>
            {
                GameInstance.ClientGuildHandlers.RequestLeaveGuild(LeaveGuildCallback);
            });
        }

        private void LeaveGuildCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseLeaveGuildMessage response)
        {
            ClientGuildActions.ResponseLeaveGuild(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
        }

        public override int GetSocialId()
        {
            return GameInstance.PlayingCharacter.GuildId;
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
            return IsLeader(GameInstance.PlayingCharacter.Id);
        }

        public override bool OwningCharacterCanKick()
        {
            return CanKick(GameInstance.PlayingCharacter.Id);
        }
    }
}
