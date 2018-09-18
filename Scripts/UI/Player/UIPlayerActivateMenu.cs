using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIPlayerActivateMenu : UISelectionEntry<BasePlayerCharacterEntity>
    {
        public UICharacter uiCharacter;
        [Tooltip("These objects will be activated when owning character is in party")]
        public GameObject[] partyObjects;

        protected override void UpdateUI()
        {
            base.UpdateUI();
            foreach (var partyObject in partyObjects)
            {
                if (partyObject != null)
                    partyObject.SetActive(BasePlayerCharacterController.OwningCharacter.PartyId > 0);
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
    }
}
