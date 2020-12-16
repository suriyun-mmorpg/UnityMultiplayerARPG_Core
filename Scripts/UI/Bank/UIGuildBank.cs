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
            GameInstance.ClientBankHandlers.RequestDepositGuildGold(new RequestDepositGuildGoldMessage()
            {
                characterId = BasePlayerCharacterController.OwningCharacter.Id,
                gold = amount,
            }, ClientBankActions.ResponseDepositGuildGold);
        }

        public override void OnWithdrawConfirm(int amount)
        {
            GameInstance.ClientBankHandlers.RequestWithdrawGuildGold(new RequestWithdrawGuildGoldMessage()
            {
                characterId = BasePlayerCharacterController.OwningCharacter.Id,
                gold = amount,
            }, ClientBankActions.ResponseWithdrawGuildGold);
        }
    }
}
