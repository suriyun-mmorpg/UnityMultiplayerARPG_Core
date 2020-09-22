namespace MultiplayerARPG
{
    public partial class UIPartyInvitation : UISelectionEntry<BasePlayerCharacterEntity>
    {
        public UICharacter uiAnotherCharacter;

        protected override void UpdateData()
        {
            if (uiAnotherCharacter != null)
            {
                uiAnotherCharacter.NotForOwningCharacter = true;
                uiAnotherCharacter.Data = Data;
            }
        }

        public void OnClickAccept()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.CallServerAcceptPartyInvitation();
            Hide();
        }

        public void OnClickDecline()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            owningCharacter.CallServerDeclinePartyInvitation();
            Hide();
        }
    }
}
