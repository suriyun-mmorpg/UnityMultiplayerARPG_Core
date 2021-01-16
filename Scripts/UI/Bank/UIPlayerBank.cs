namespace MultiplayerARPG
{
    public class UIPlayerBank : UIBaseBank
    {
        public override int GetAmount()
        {
            if (GameInstance.PlayingCharacter == null)
                return 0;
            return GameInstance.PlayingCharacter.UserGold;
        }

        public override void OnDepositConfirm(int amount)
        {
            GameInstance.ClientBankHandlers.RequestDepositUserGold(new RequestDepositUserGoldMessage()
            {
                gold = amount,
            }, ClientBankActions.ResponseDepositUserGold);
        }

        public override void OnWithdrawConfirm(int amount)
        {
            GameInstance.ClientBankHandlers.RequestWithdrawUserGold(new RequestWithdrawUserGoldMessage()
            {
                gold = amount,
            }, ClientBankActions.ResponseWithdrawUserGold);
        }
    }
}
