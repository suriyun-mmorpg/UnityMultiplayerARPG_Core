namespace MultiplayerARPG
{
    public partial class BaseRewardDropEntity
    {
        public override void Clean()
        {
            base.Clean();
            for (int i = 0; i < appearanceSettings.Count; ++i)
            {
                appearanceSettings[i].Clean();
            }
            appearanceSettings?.Clear();
            onPickedUp?.RemoveAllListeners();
            onPickedUp = null;
            Looters?.Clear();
            SpawnArea = null;
            SpawnPrefab = null;
            SpawnAddressablePrefab = null;
            _allActivatingObjects.Nulling();
            _allActivatingObjects?.Clear();
            _allActivatingObjects = null;
        }
    }
}