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
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.CallServerAcceptDealingRequest();
            Hide();
        }

        public void OnClickDecline()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.CallServerDeclineDealingRequest();
            Hide();
        }
    }
}
