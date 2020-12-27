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
