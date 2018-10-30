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

        private BaseGameNetworkManager cacheGameNetworkManager;
        public BaseGameNetworkManager CacheGameNetworkManager
        {
            get
            {
                if (cacheGameNetworkManager == null)
                    cacheGameNetworkManager = FindObjectOfType<BaseGameNetworkManager>();
                return cacheGameNetworkManager;
            }
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();
            foreach (var obj in partyInviteObjects)
            {
                if (obj != null)
                    obj.SetActive(CacheGameNetworkManager.ClientParty != null && CacheGameNetworkManager.ClientParty.CanInvite(BasePlayerCharacterController.OwningCharacter.Id));
            }
            foreach (var obj in guildInviteObjects)
            {
                if (obj != null)
                    obj.SetActive(CacheGameNetworkManager.ClientGuild != null && CacheGameNetworkManager.ClientGuild.CanInvite(BasePlayerCharacterController.OwningCharacter.Id));
            }
        }

        protected override void UpdateData()
        {
            if (uiCharacter != null)
                uiCharacter.Data = Data;
        }

        public void OnClickSendDealingRequest()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestSendDealingRequest(Data.ObjectId);
            Hide();
        }

        public void OnClickSendPartyInvitation()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestSendPartyInvitation(Data.ObjectId);
            Hide();
        }

        public void OnClickSendGuildInvitation()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestSendGuildInvitation(Data.ObjectId);
            Hide();
        }
    }
}
