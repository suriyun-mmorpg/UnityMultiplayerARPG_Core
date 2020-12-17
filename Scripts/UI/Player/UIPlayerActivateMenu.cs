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
            base.UpdateUI();
            foreach (GameObject obj in partyInviteObjects)
            {
                if (obj != null)
                    obj.SetActive(GameInstance.ClientPartyHandlers.ClientParty != null && GameInstance.ClientPartyHandlers.ClientParty.CanInvite(GameInstance.ClientUserHandlers.CharacterId));
            }
            foreach (GameObject obj in guildInviteObjects)
            {
                if (obj != null)
                    obj.SetActive(GameInstance.ClientGuildHandlers.ClientGuild != null && GameInstance.ClientGuildHandlers.ClientGuild.CanInvite(GameInstance.ClientUserHandlers.CharacterId));
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
            BasePlayerCharacterController.OwningCharacter.CallServerSendDealingRequest(Data.ObjectId);
            Hide();
        }

        public void OnClickSendPartyInvitation()
        {
            GameInstance.ClientPartyHandlers.RequestSendPartyInvitation(new RequestSendPartyInvitationMessage()
            {
                characterId = GameInstance.ClientUserHandlers.CharacterId,
                inviteeId = Data.Id,
            }, ClientPartyActions.ResponseSendPartyInvitation);
            Hide();
        }

        public void OnClickSendGuildInvitation()
        {
            GameInstance.ClientGuildHandlers.RequestSendGuildInvitation(new RequestSendGuildInvitationMessage()
            {
                characterId = GameInstance.ClientUserHandlers.CharacterId,
                inviteeId = Data.Id,
            }, ClientGuildActions.ResponseSendGuildInvitation);
            Hide();
        }
    }
}
