namespace MultiplayerARPG
{
    public partial class UIConstructBuilding : UIBase
    {
        public BasePlayerCharacterController Controller { get { return BasePlayerCharacterController.Singleton; } }
        public TextWrapper textTitle;

        public override void Show()
        {
            if (Controller.ConstructingBuildingEntity == null)
            {
                // Don't show
                return;
            }
            base.Show();
        }

        protected virtual void OnEnable()
        {
            if (textTitle != null)
                textTitle.text = Controller.ConstructingBuildingEntity.Title;
        }

        public void OnClickConfirmBuild()
        {
            Controller.ConfirmBuild();
            Hide();
        }

        public void OnClickCancelBuild()
        {
            Controller.CancelBuild();
            Hide();
        }
    }
}
