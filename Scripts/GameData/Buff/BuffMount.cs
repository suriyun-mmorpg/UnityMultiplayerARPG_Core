using Insthync.AddressableAssetTools;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class BuffMount : IAddressableAssetConversable
    {
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [Tooltip("Leave `Mount Entity` to NULL to not summon mount entity")]
        [SerializeField]
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

        [SerializeField]
        private IncrementalInt level;
        public IncrementalInt Level { get { return level; } }

        public void ProceedAddressableAssetConversion(string groupName)
        {
#if UNITY_EDITOR
            AddressableEditorUtils.ConvertObjectRefToAddressable(ref mountEntity, ref addressableMountEntity, groupName);
#endif
        }
    }
}