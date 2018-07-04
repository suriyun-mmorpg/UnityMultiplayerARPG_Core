using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIPlayerActivateMenu : UISelectionEntry<BasePlayerCharacterEntity>
    {
        public UICharacter uiCharacter;

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

        public void OnClickSendDuelRequest()
        {
            // TODO: Will implement it soon
        }
    }
}
