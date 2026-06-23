using UnityEngine;

namespace MultiplayerARPG
{
    public partial class HarvestableEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                harvestable = null;
                onHarvestableDestroy?.RemoveAllListeners();
                onHarvestableDestroy = null;
            }
            SpawnArea = null;
            SpawnPrefab = null;
#if !DISABLE_ADDRESSABLES
            SpawnAddressablePrefab = null;
#endif
            SpawnLevel = 0;
            SpawnPosition = Vector3.zero;
            _isDestroyed = false;
        }
    }
}
