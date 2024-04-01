using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct SkillMount
    {
        public static readonly SkillMount Empty = new SkillMount();
#if UNITY_EDITOR || !LNLM_NO_PREFABS
        [Tooltip("Leave `Mount Entity` to NULL to not summon mount entity")]
        [SerializeField]
        private VehicleEntity mountEntity;
#endif
        public VehicleEntity MountEntity
        {
            get
            {
#if !LNLM_NO_PREFABS
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
#if UNITY_EDITOR || !LNLM_NO_PREFABS
            this.mountEntity = mountEntity;
#endif
            this.addressableMountEntity = addressableMountEntity;
        }
    }
}
