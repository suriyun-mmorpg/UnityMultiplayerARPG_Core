using UnityEngine;

namespace MultiplayerARPG
{
    public partial class ItemDropEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                modelContainer = null;
                onPickedUp?.RemoveAllListeners();
                onPickedUp = null;
            }
            PutOnPlaceholder = false;
            GivenType = RewardGivenType.None;
            DropItems?.Clear();
            Looters?.Clear();
            SpawnArea = null;
            SpawnPrefab = null;
#if !DISABLE_ADDRESSABLES
            SpawnAddressablePrefab = null;
#endif
            SpawnLevel = 0;
            SpawnPosition = Vector3.zero;
            if (_dropModel != null)
                Destroy(_dropModel);
            _dropModel = null;
            _isPickedUp = false;
            _dropTime = 0f;
        }
    }
}
