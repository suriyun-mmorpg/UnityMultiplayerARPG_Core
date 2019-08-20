using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildBank : UIBaseBank
    {
        public override int GetAmount()
        {
            if (BaseGameNetworkManager.ClientGuild == null)
                return 0;
            return BaseGameNetworkManager.ClientGuild.gold;
        }

        public override void OnDepositConfirm(int amount)
        {
            BasePlayerCharacterController.OwningCharacter.RequestDepositGuildGold(amount);
        }

        public override void OnWithdrawConfirm(int amount)
        {
            BasePlayerCharacterController.OwningCharacter.RequestWithdrawGuildGold(amount);
        }
    }
}
