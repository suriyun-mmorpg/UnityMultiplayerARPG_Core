using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIPlayerBank : UIBaseBank
    {
        public override int GetAmount()
        {
            return BasePlayerCharacterController.OwningCharacter.UserGold;
        }

        public override void OnDepositConfirm(int amount)
        {
            BasePlayerCharacterController.OwningCharacter.CallServerDepositGold(amount);
        }

        public override void OnWithdrawConfirm(int amount)
        {
            BasePlayerCharacterController.OwningCharacter.CallServerWithdrawGold(amount);
        }
    }
}
