namespace MultiplayerARPG
{
    public partial class ItemsContainerEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                onPickedUp?.RemoveAllListeners();
                onPickedUp = null;
            }
            GivenType = RewardGivenType.None;
            Looters?.Clear();
            _isDestroyed = false;
            _dropTime = 0f;
            _appearDuration = 0f;
        }
    }
}
