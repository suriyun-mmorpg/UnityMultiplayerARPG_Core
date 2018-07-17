using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDealingRequest : UISelectionEntry<BasePlayerCharacterEntity>
    {
        public UICharacter uiAnotherCharacter;

        protected override void UpdateData()
        {
            var anotherCharacter = Data;

            if (uiAnotherCharacter != null)
                uiAnotherCharacter.Data = anotherCharacter;
        }

        public void OnClickAccept()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestAcceptDealingRequest();
            Hide();
        }

        public void OnClickDecline()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestDeclineDealingRequest();
            Hide();
        }
    }
}
