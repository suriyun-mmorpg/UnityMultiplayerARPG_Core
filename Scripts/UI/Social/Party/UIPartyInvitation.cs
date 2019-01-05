namespace MultiplayerARPG
{
    public partial class UIPartyInvitation : UISelectionEntry<BasePlayerCharacterEntity>
    {
        public UICharacter uiAnotherCharacter;

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity anotherCharacter = Data;

            if (uiAnotherCharacter != null)
                uiAnotherCharacter.Data = anotherCharacter;
        }

        public void OnClickAccept()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestAcceptPartyInvitation();
            Hide();
        }

        public void OnClickDecline()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.RequestDeclinePartyInvitation();
            Hide();
        }
    }
}
