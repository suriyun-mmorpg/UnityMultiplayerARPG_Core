using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.PLAYER_CHARACTER_ENTITY_METADATA_FILE, menuName = GameDataMenuConsts.PLAYER_CHARACTER_ENTITY_METADATA_MENU, order = GameDataMenuConsts.PLAYER_CHARACTER_ENTITY_METADATA_ORDER)]
    public partial class PlayerCharacterEntityMetaData : BaseGameData
    {
        [SerializeField]
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [AddressableAssetConversion(nameof(addressableEntityPrefab))]
        private BasePlayerCharacterEntity entityPrefab;
#endif
        public BasePlayerCharacterEntity EntityPrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return entityPrefab;
#else
                return null;
#endif
            }
        }

        [SerializeField]
        private AssetReferenceBasePlayerCharacterEntity addressableEntityPrefab;
        public AssetReferenceBasePlayerCharacterEntity AddressableEntityPrefab
        {
            get
            {
                return addressableEntityPrefab;
            }
        }

        public int GetPlayerCharacterEntityHashAssetId()
        {
            if (AddressableEntityPrefab.IsDataValid())
                return AddressableEntityPrefab.HashAssetId;
            if (EntityPrefab != null)
                return EntityPrefab.HashAssetId;
            return 0;
        }
    }
}
