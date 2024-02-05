using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class EquipmentModel
    {
        [Header("Generic Settings")]
        public string equipSocket;

        [Header("Prefab Settings")]
        [Tooltip("Turn it on to use instantiated object which is a child of character model")]
        [FormerlySerializedAs("useInstantiatedObject")]
        public bool useInstantiatedObject;
        [BoolShowConditional(nameof(useInstantiatedObject), false)]
        [FormerlySerializedAs("model")]
        public GameObject meshPrefab;
        [BoolShowConditional(nameof(useInstantiatedObject), true)]
        public int instantiatedObjectIndex;
        public byte priority;

        [Header("Skinned Mesh Settings")]
        [Tooltip("Turn it on to not use bones from entities if this mesh is skinned mesh")]
        [FormerlySerializedAs("doNotUseEntityBones")]
        public bool doNotSetupBones;
        [Tooltip("Leave this empty, to use `EquipmentModelBonesSetupManager` from `GameInstance`")]
        [BoolShowConditional(nameof(doNotSetupBones), false)]
        public BaseEquipmentModelBonesSetupManager equipmentModelBonesSetupManager;

        [Header("Transform Settings")]
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localEulerAngles = Vector3.zero;
        [Tooltip("Turn it on to not change object scale when it is instantiated to character's hands (or other part of body)")]
        public bool doNotChangeScale;
        [BoolShowConditional(nameof(doNotChangeScale), false)]
        public Vector3 localScale = Vector3.one;

        [Header("Weapon Sheath Settings")]
        public bool useSpecificSheathEquipWeaponSet;
        public byte specificSheathEquipWeaponSet;

        #region These variables will be used at runtime, do not make changes in editor
        [HideInInspector]
        public int indexOfModel = -1;
        [HideInInspector]
        public string equipPosition;
        [HideInInspector]
        public CharacterItem item;
        [HideInInspector]
        public EquipmentModelDelegate onInstantiated;
        #endregion

        public EquipmentModel Clone()
        {
            return new EquipmentModel()
            {
                // Generic Settings
                equipSocket = equipSocket,
                // Prefab Settings
                useInstantiatedObject = useInstantiatedObject,
                meshPrefab = meshPrefab,
                instantiatedObjectIndex = instantiatedObjectIndex,
                priority = priority,
                // Skinned Mesh Settings
                doNotSetupBones = doNotSetupBones,
                equipmentModelBonesSetupManager = equipmentModelBonesSetupManager,
                // Transform Settings
                localPosition = localPosition,
                localEulerAngles = localEulerAngles,
                doNotChangeScale = doNotChangeScale,
                localScale = localScale,
                // Weapon Sheath Settings
                useSpecificSheathEquipWeaponSet = useSpecificSheathEquipWeaponSet,
                specificSheathEquipWeaponSet = specificSheathEquipWeaponSet,
                // Runtime only data
                indexOfModel = indexOfModel,
                equipPosition = equipPosition,
                item = item,
                onInstantiated = onInstantiated,
            };
        }

        public void InvokeOnInstantiated(GameObject modelObject, BaseEquipmentEntity equipmentEntity, EquipmentInstantiatedObjectGroup instantiatedObjectGroup, EquipmentContainer equipmentContainer)
        {
            if (onInstantiated != null)
                onInstantiated.Invoke(this, modelObject, equipmentEntity, instantiatedObjectGroup, equipmentContainer);
        }
    }
}
