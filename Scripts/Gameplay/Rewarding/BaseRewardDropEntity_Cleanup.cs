using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseRewardDropEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                for (int i = 0; i < appearanceSettings.Count; ++i)
                {
                    appearanceSettings[i].Clean();
                }
                appearanceSettings?.Clear();
                onPickedUp?.RemoveAllListeners();
                onPickedUp = null;
                _allActivatingObjects.Nullify();
                _allActivatingObjects?.Clear();
            }
            Multiplier = 0f;
            GivenType = RewardGivenType.None;
            GiverLevel = 0;
            SourceLevel = 0;
            Looters?.Clear();
            SpawnArea = null;
            SpawnPrefab = null;
#if !DISABLE_ADDRESSABLES
            SpawnAddressablePrefab = null;
#endif
            SpawnLevel = 0;
            SpawnPosition = Vector3.zero;
            _isPickedUp = false;
            _dropTime = 0f;
        }
    }
}