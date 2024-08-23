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
        protected BasePlayerCharacterEntity entityPrefab;
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
        protected AssetReferenceBasePlayerCharacterEntity addressableEntityPrefab;
        public AssetReferenceBasePlayerCharacterEntity AddressableEntityPrefab
        {
            get
            {
                return addressableEntityPrefab;
            }
        }

        [Tooltip("This is list which used as choice of character classes when create character")]
        [SerializeField]
        protected PlayerCharacter[] characterDatabases = new PlayerCharacter[0];
        public PlayerCharacter[] CharacterDatabases
        {
            get { return characterDatabases; }
            set { characterDatabases = value; }
        }

#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [Tooltip("Leave this empty to use GameInstance's controller prefab")]
        [SerializeField]
        protected BasePlayerCharacterController controllerPrefab;
#endif
        public BasePlayerCharacterController ControllerPrefab
        {
            get
            {
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
                return controllerPrefab;
#else
                return null;
#endif
            }
            set
            {
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
                controllerPrefab = value;
#endif
            }
        }

        [Tooltip("Leave this empty to use GameInstance's controller prefab")]
        [SerializeField]
        protected AssetReferenceBasePlayerCharacterController addressableControllerPrefab;
        public AssetReferenceBasePlayerCharacterController AddressableControllerPrefab
        {
            get { return addressableControllerPrefab; }
            set { addressableControllerPrefab = value; }
        }

        public int GetPlayerCharacterEntityHashAssetId()
        {
            if (AddressableEntityPrefab.IsDataValid())
                return AddressableEntityPrefab.HashAssetId;
            if (EntityPrefab != null)
                return EntityPrefab.HashAssetId;
            return 0;
        }

        public async UniTask<BasePlayerCharacterEntity> GetOrLoadAssetAsync()
        {
            BasePlayerCharacterEntity loadedPrefab = await AddressableEntityPrefab.GetOrLoadAssetAsyncOrUsePrefab(EntityPrefab);
            loadedPrefab.CharacterDatabases = characterDatabases;
            loadedPrefab.ControllerPrefab = controllerPrefab;
            loadedPrefab.AddressableControllerPrefab = AddressableControllerPrefab;
            return loadedPrefab;
        }
    }
}
