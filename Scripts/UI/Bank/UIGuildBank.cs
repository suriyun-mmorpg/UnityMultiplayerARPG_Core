using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildBank : UIBaseBank
    {
        public override int GetAmount()
        {
            if (GameInstance.ClientGuildHandlers.ClientGuild == null)
                return 0;
            return GameInstance.ClientGuildHandlers.ClientGuild.gold;
        }

        public override void OnDepositConfirm(int amount)
        {
            BasePlayerCharacterController.OwningCharacter.CallServerDepositGuildGold(amount);
        }

        public override void OnWithdrawConfirm(int amount)
        {
            BasePlayerCharacterController.OwningCharacter.CallServerWithdrawGuildGold(amount);
        }
    }
}
