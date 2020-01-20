namespace MultiplayerARPG
{
    public partial class UIConstructBuilding : UIBase
    {
        public void OnClickConfirmBuild()
        {
            BasePlayerCharacterController.Singleton.ConfirmBuild();
            Hide();
        }

        public void OnClickCancelBuild()
        {
            BasePlayerCharacterController.Singleton.CancelBuild();
            Hide();
        }
    }
}
