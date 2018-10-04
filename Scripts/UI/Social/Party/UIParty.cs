using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;

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
        public float refreshDuration = 1f;
        private float lastRefreshTime;

        public bool shareExp { get; private set; }
        public bool shareItem { get; private set; }

        protected override void Update()
        {
            base.Update();

            // Refresh party info
            if (currentSocialId > 0)
            {
                if (Time.unscaledTime - lastRefreshTime >= refreshDuration)
                {
                    lastRefreshTime = Time.unscaledTime;
                    RefreshPartyInfo();
                }
            }
        }

        protected override void UpdateUIs()
        {
            if (toggleShareExp != null)
            {
                toggleShareExp.interactable = false;
                toggleShareExp.isOn = shareExp;
            }

            if (toggleShareItem != null)
            {
                toggleShareItem.interactable = false;
                toggleShareItem.isOn = shareItem;
            }

            base.UpdateUIs();
        }

        public void RefreshPartyInfo()
        {
            // Load cash shop item list
            CacheGameNetworkManager.RequestPartyData(ResponsePartyInfo);
        }

        public override void Show()
        {
            base.Show();
            RefreshPartyInfo();
        }

        public override void Hide()
        {
            if (uiPartyCreate != null)
                uiPartyCreate.Hide();
            if (uiPartySetting != null)
                uiPartySetting.Hide();
            base.Hide();
        }

        private void ResponsePartyInfo(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponsePartyDataMessage)message;
            if (responseCode == AckResponseCode.Success)
            {
                shareExp = castedMessage.shareExp;
                shareItem = castedMessage.shareItem;
                memberAmount = castedMessage.members.Length;
                UpdateUIs();

                var selectedIdx = MemberSelectionManager.SelectedUI != null ? MemberSelectionManager.IndexOf(MemberSelectionManager.SelectedUI) : -1;
                MemberSelectionManager.DeselectSelectedUI();
                MemberSelectionManager.Clear();

                MemberList.Generate(castedMessage.members, (index, partyMember, ui) =>
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
            if (!OwningCharacterIsLeader())
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
            if (!OwningCharacterIsLeader())
                return;

            // Show setup party dialog
            if (uiPartySetting != null)
                uiPartySetting.Show(shareExp, shareItem);
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

        public override bool IsLeader(byte flags)
        {
            return PartyData.IsLeader((PartyMemberFlags)flags);
        }

        public override bool CanKick(byte flags)
        {
            return PartyData.CanKick((PartyMemberFlags)flags);
        }

        public override bool OwningCharacterIsLeader()
        {
            return PartyData.IsLeader(BasePlayerCharacterController.OwningCharacter.PartyMemberFlags);
        }

        public override bool OwningCharacterCanKick()
        {
            return PartyData.CanKick(BasePlayerCharacterController.OwningCharacter.PartyMemberFlags);
        }
    }
}
