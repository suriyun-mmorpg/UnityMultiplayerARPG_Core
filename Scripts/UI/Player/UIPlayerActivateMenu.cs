using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIPlayerActivateMenu : UISelectionEntry<BasePlayerCharacterEntity>
    {
        public UICharacter uiCharacter;
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
                    obj.SetActive(BaseGameNetworkManager.Singleton.ClientParty != null && BaseGameNetworkManager.Singleton.ClientParty.CanInvite(BasePlayerCharacterController.OwningCharacter.Id));
            }
            foreach (GameObject obj in guildInviteObjects)
            {
                if (obj != null)
                    obj.SetActive(BaseGameNetworkManager.Singleton.ClientGuild != null && BaseGameNetworkManager.Singleton.ClientGuild.CanInvite(BasePlayerCharacterController.OwningCharacter.Id));
            }
        }

        protected override void UpdateData()
        {
            if (uiCharacter != null)
                uiCharacter.Data = Data;
        }

        public void OnClickSendDealingRequest()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestSendDealingRequest(Data.ObjectId);
            Hide();
        }

        public void OnClickSendPartyInvitation()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestSendPartyInvitation(Data.ObjectId);
            Hide();
        }

        public void OnClickSendGuildInvitation()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestSendGuildInvitation(Data.ObjectId);
            Hide();
        }
    }
}
