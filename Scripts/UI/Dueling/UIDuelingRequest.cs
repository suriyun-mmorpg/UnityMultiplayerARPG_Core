namespace MultiplayerARPG
{
    public partial class UIDuelingRequest : UISelectionEntry<BasePlayerCharacterEntity>
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
            GameInstance.PlayingCharacterEntity.Dueling.CallCmdAcceptDuelingRequest();
            Hide();
        }

        public void OnClickDecline()
        {
            GameInstance.PlayingCharacterEntity.Dueling.CallCmdDeclineDuelingRequest();
            Hide();
        }
    }
}
