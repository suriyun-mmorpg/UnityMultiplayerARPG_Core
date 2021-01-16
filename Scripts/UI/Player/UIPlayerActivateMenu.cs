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
                    obj.SetActive(GameInstance.ClientParty != null && GameInstance.ClientParty.CanInvite(GameInstance.Character.Id));
            }
            foreach (GameObject obj in guildInviteObjects)
            {
                if (obj != null)
                    obj.SetActive(GameInstance.ClientGuild != null && GameInstance.ClientGuild.CanInvite(GameInstance.Character.Id));
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
                inviteeId = Data.Id,
            }, ClientPartyActions.ResponseSendPartyInvitation);
            Hide();
        }

        public void OnClickSendGuildInvitation()
        {
            GameInstance.ClientGuildHandlers.RequestSendGuildInvitation(new RequestSendGuildInvitationMessage()
            {
                inviteeId = Data.Id,
            }, ClientGuildActions.ResponseSendGuildInvitation);
            Hide();
        }
    }
}
