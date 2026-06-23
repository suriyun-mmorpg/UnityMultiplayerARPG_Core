using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseMonsterCharacterEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                characterDatabase = null;
                faction = null;
                InstantiatedObjects.DestroyAndNullify();
                InstantiatedObjects.Clear();
                _isObjectsInstantiated = false;
            }
            SpawnArea = null;
            SpawnPrefab = null;
#if !DISABLE_ADDRESSABLES
            SpawnAddressablePrefab = null;
#endif
            SpawnLevel = 0;
            SpawnPosition = Vector3.zero;
            _isDestroyed = false;
            _looters?.Clear();
            _droppingItems?.Clear();
            _killedReward = null;
            _lastTeleportToSummonerTime = 0f;
        }
    }
}