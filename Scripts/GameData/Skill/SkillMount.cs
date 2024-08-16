using Insthync.AddressableAssetTools;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct SkillMount
    {
        public static readonly SkillMount Empty = new SkillMount();
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [Tooltip("Leave `Mount Entity` to NULL to not summon mount entity")]
        [SerializeField]
        [AddressableAssetConversion(nameof(addressableMountEntity))]
        private VehicleEntity mountEntity;
#endif
        public VehicleEntity MountEntity
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return mountEntity;
#else
                return null;
#endif
            }
        }

        [SerializeField]
        private AssetReferenceVehicleEntity addressableMountEntity;
        public AssetReferenceVehicleEntity AddressableMountEntity
        {
            get
            {
                return addressableMountEntity;
            }
        }

        public SkillMount(VehicleEntity mountEntity, AssetReferenceVehicleEntity addressableMountEntity)
        {
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
            this.mountEntity = mountEntity;
#endif
            this.addressableMountEntity = addressableMountEntity;
        }
    }
}
