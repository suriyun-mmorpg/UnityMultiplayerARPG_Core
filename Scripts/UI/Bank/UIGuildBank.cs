using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildBank : UIBaseBank
    {
        public override int GetAmount()
        {
            return BaseGameNetworkManager.Singleton.ClientGuild.gold;
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
