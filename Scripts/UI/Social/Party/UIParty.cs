using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UISocialCharacterSelectionManager))]
    public class UIParty : UISocialGroup<UISocialCharacter>
    {
        [Header("UI Elements")]
        public Toggle toggleShareExp;
        public Toggle toggleShareItem;
        public UIPartyCreate uiPartyCreate;
        public UIPartySetting uiPartySetting;

        public PartyData Party { get { return CacheGameNetworkManager.ClientParty; } }

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
            CacheGameNetworkManager.onClientUpdateParty += UpdatePartyUIs;
        }

        private void OnDisable()
        {
            CacheGameNetworkManager.onClientUpdateParty -= UpdatePartyUIs;
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
            
            var selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
            MemberSelectionManager.DeselectSelectedUI();
            MemberSelectionManager.Clear();

            SocialCharacterData[] members;
            party.GetSortedMembers(out members);
            MemberList.Generate(members, (index, partyMember, ui) =>
            {
                var partyMemberEntity = new SocialCharacterEntityTuple();
                partyMemberEntity.socialCharacter = partyMember;

                var uiPartyMember = ui.GetComponent<UISocialCharacter>();
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

            var partyMember = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog("Change Leader", string.Format("You sure you want to promote {0} to party leader?", partyMember.characterName), false, true, false, false, null, () =>
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

            var partyMember = MemberSelectionManager.SelectedUI.Data.socialCharacter;
            UISceneGlobal.Singleton.ShowMessageDialog("Kick Member", string.Format("You sure you want to kick {0} from party?", partyMember.characterName), false, true, false, false, null, () =>
            {
                BasePlayerCharacterController.OwningCharacter.RequestKickFromParty(partyMember.id);
            });
        }

        public void OnClickLeaveParty()
        {
            UISceneGlobal.Singleton.ShowMessageDialog("Leave Party", "You sure you want to leave party?", false, true, false, false, null, () =>
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
            return GameInstance.Singleton.SocialSystemSetting.MaxPartyMember;
        }

        public override bool IsLeader(string characterId)
        {
            return CacheGameNetworkManager.ClientParty != null && CacheGameNetworkManager.ClientParty.IsLeader(characterId);
        }

        public override bool IsOnline(string characterId)
        {
            return CacheGameNetworkManager.ClientParty != null && CacheGameNetworkManager.ClientParty.IsOnline(characterId);
        }

        public override bool CanKick(string characterId)
        {
            return CacheGameNetworkManager.ClientParty != null && CacheGameNetworkManager.ClientParty.CanKick(characterId);
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
