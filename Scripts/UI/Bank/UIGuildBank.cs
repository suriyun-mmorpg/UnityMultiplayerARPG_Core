namespace MultiplayerARPG
{
    public class UIGuildBank : UIBaseBank
    {
        public override int GetAmount()
        {
            if (GameInstance.JoinedGuild == null)
                return 0;
            return GameInstance.JoinedGuild.gold;
        }

        public override void OnDepositConfirm(int amount)
        {
            GameInstance.ClientBankHandlers.RequestDepositGuildGold(new RequestDepositGuildGoldMessage()
            {
                gold = amount,
            }, ClientBankActions.ResponseDepositGuildGold);
        }

        public override void OnWithdrawConfirm(int amount)
        {
            GameInstance.ClientBankHandlers.RequestWithdrawGuildGold(new RequestWithdrawGuildGoldMessage()
            {
                gold = amount,
            }, ClientBankActions.ResponseWithdrawGuildGold);
        }
    }
}
