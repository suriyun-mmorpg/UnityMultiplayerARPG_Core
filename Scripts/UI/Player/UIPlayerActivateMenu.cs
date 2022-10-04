using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UIPlayerActivateMenu : UISelectionEntry<BasePlayerCharacterEntity>
    {
        [FormerlySerializedAs("uiCharacter")]
        public UICharacter uiAnotherCharacter;
        [Tooltip("These objects will be activated when owning character can invite to join party")]
        public GameObject[] partyInviteObjects;
        [Tooltip("These objects will be activated when owning character can invite to join guild")]
        public GameObject[] guildInviteObjects;

        protected override void UpdateUI()
        {
            if (Data == null)
            {
                Hide();
                return;
            }
            base.UpdateUI();
            foreach (GameObject obj in partyInviteObjects)
            {
                if (obj != null)
                    obj.SetActive(Data.PartyId <= 0 && GameInstance.JoinedParty != null && GameInstance.JoinedParty.CanInvite(GameInstance.PlayingCharacter.Id));
            }
            foreach (GameObject obj in guildInviteObjects)
            {
                if (obj != null)
                    obj.SetActive(Data.GuildId <= 0 && GameInstance.JoinedGuild != null && GameInstance.JoinedGuild.CanInvite(GameInstance.PlayingCharacter.Id));
            }
        }

        protected override void UpdateData()
        {
            if (uiAnotherCharacter != null)
            {
                uiAnotherCharacter.NotForOwningCharacter = true;
                uiAnotherCharacter.Data = Data;
            }
        }

        public void OnClickSendDealingRequest()
        {
            GameInstance.PlayingCharacterEntity.Dealing.CallServerSendDealingRequest(Data.ObjectId);
            Hide();
        }

        public void OnClickSendPartyInvitation()
        {
            GameInstance.ClientPartyHandlers.RequestSendPartyInvitation(new RequestSendPartyInvitationMessage()
            {
                inviteeId = Data.Id,
            }, SendPartyInvitationCallback);
        }

        public void SendPartyInvitationCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSendPartyInvitationMessage response)
        {
            ClientPartyActions.ResponseSendPartyInvitation(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message))
                return;
            Hide();
        }

        public void OnClickSendGuildInvitation()
        {
            GameInstance.ClientGuildHandlers.RequestSendGuildInvitation(new RequestSendGuildInvitationMessage()
            {
                inviteeId = Data.Id,
            }, SendGuildInvitationCallback);
        }

        public void SendGuildInvitationCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSendGuildInvitationMessage response)
        {
            ClientGuildActions.ResponseSendGuildInvitation(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message))
                return;
            Hide();
        }
    }
}
