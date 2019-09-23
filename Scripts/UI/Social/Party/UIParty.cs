using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIParty : UISocialGroup<UISocialCharacter>
    {
        [Header("UI Elements")]
        public Toggle toggleShareExp;
        public Toggle toggleShareItem;
        public UIPartyCreate uiPartyCreate;
        public UIPartySetting uiPartySetting;

        public PartyData Party { get { return BaseGameNetworkManager.ClientParty; } }

        protected override void UpdateUIs()
        {
            if (toggleShareExp != null)
            {
                toggleShareExp.interactable = false;
                toggleShareExp.isOn = Party != null && Party.shareExp;
            }

            if (toggleShareItem != null)
            {
                toggleShareItem.interactable = false;
                toggleShareItem.isOn = Party != null && Party.shareItem;
            }

            base.UpdateUIs();
        }

        private void OnEnable()
        {
            BaseGameNetworkManager.Singleton.onClientUpdateParty += UpdatePartyUIs;
        }

        private void OnDisable()
        {
            BaseGameNetworkManager.Singleton.onClientUpdateParty -= UpdatePartyUIs;
        }

        public override void Show()
        {
            base.Show();
            UpdatePartyUIs(Party);
        }

        public override void Hide()
        {
            if (uiPartyCreate != null)
                uiPartyCreate.Hide();
            if (uiPartySetting != null)
                uiPartySetting.Hide();
            base.Hide();
        }

        private void UpdatePartyUIs(PartyData party)
        {
            if (party == null)
                return;

            memberAmount = party.CountMember();
            UpdateUIs();

            int selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            SocialCharacterData[] members;
            party.GetSortedMembers(out members);
            MemberList.Generate(members, (index, partyMember, ui) =>
            {
                UISocialCharacterData partyMemberEntity = new UISocialCharacterData();
                partyMemberEntity.socialCharacter = partyMember;

                UISocialCharacter uiPartyMember = ui.GetComponent<UISocialCharacter>();
                uiPartyMember.uiSocialGroup = this;
                uiPartyMember.Data = partyMemberEntity;
                uiPartyMember.Show();
                MemberSelectionManager.Add(uiPartyMember);
                if (selectedIdx == index)
                    uiPartyMember.OnClickSelect();
            });
        }

        public void OnClickCreateParty()
        {
            // If already in the party, return
            if (currentSocialId > 0)
                return;
            // Show create party dialog
            if (uiPartyCreate != null)
                uiPartyCreate.Show(false, false);
        }

        public void OnClickChangeLeader()
        {
            // If not in the party or not leader, return
            if (!OwningCharacterIsLeader() || MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData partyMember = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_PARTY_CHANGE_LEADER.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_PARTY_CHANGE_LEADER_DESCRIPTION.ToString()), partyMember.characterName), false, true, true, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestChangePartyLeader(partyMember.id);
            });
        }

        public void OnClickSettingParty()
        {
            // If not in the party or not leader, return
            if (!OwningCharacterIsLeader() && Party != null)
                return;

            // Show setup party dialog
            if (uiPartySetting != null)
                uiPartySetting.Show(Party.shareExp, Party.shareItem);
        }

        public void OnClickKickFromParty()
        {
            // If not in the party or not leader, return
            if (!OwningCharacterCanKick() || MemberSelectionManager.SelectedUI == null)
                return;

            SocialCharacterData partyMember = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_PARTY_KICK_MEMBER.ToString()), string.Format(LanguageManager.GetText(UITextKeys.UI_PARTY_KICK_MEMBER_DESCRIPTION.ToString()), partyMember.characterName), false, true, true, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestKickFromParty(partyMember.id);
            });
        }

        public void OnClickLeaveParty()
        {
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_PARTY_LEAVE.ToString()), LanguageManager.GetText(UITextKeys.UI_PARTY_LEAVE_DESCRIPTION.ToString()), false, true, true, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestLeaveParty();
            });
        }

        public override int GetSocialId()
        {
            return BasePlayerCharacterController.OwningCharacter.PartyId;
        }

        public override int GetMaxMemberAmount()
        {
            if (Party == null)
                return 0;
            return Party.MaxMember();
        }

        public override bool IsLeader(string characterId)
        {
            return Party != null && Party.IsLeader(characterId);
        }

        public override bool CanKick(string characterId)
        {
            return Party != null && Party.CanKick(characterId);
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
