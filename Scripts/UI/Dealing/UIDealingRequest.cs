namespace MultiplayerARPG
{
    public partial class UIDealingRequest : UISelectionEntry<BasePlayerCharacterEntity>
    {
        public UICharacter uiAnotherCharacter;

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity anotherCharacter = Data;

            if (uiAnotherCharacter != null)
            {
                uiAnotherCharacter.NotForOwningCharacter = true;
                uiAnotherCharacter.Data = anotherCharacter;
            }
        }

        public void OnClickAccept()
        {
            GameInstance.PlayingCharacterEntity.Dealing.CallServerAcceptDealingRequest();
            Hide();
        }

        public void OnClickDecline()
        {
            GameInstance.PlayingCharacterEntity.Dealing.CallServerDeclineDealingRequest();
            Hide();
        }
    }
}
