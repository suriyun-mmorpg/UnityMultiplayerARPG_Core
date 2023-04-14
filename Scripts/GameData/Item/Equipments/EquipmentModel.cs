using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct EquipmentModel
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
        public bool doNotUseEntityBones;

        [Header("Transform Settings")]
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
        [Tooltip("Turn it on to not change object scale when it is instantiated to character's hands (or other part of body)")]
        public bool doNotChangeScale;
        [BoolShowConditional(nameof(doNotChangeScale), false)]
        public Vector3 localScale;

        [Header("Weapon Sheath Settings")]
        public bool useSpecificSheathEquipWeaponSet;
        public byte specificSheathEquipWeaponSet;

        #region These variables will be used at runtime, do not make changes in editor
        [HideInInspector]
        public int itemDataId;
        [HideInInspector]
        public int itemLevel;
        [HideInInspector]
        public string equipPosition;
        #endregion

        public EquipmentModel Clone()
        {
            return new EquipmentModel()
            {
                equipSocket = equipSocket,
                useInstantiatedObject = useInstantiatedObject,
                meshPrefab = meshPrefab,
                instantiatedObjectIndex = instantiatedObjectIndex,
                localPosition = localPosition,
                localEulerAngles = localEulerAngles,
                localScale = localScale,
                priority = priority,
                // Runtime only data
                itemDataId = itemDataId,
                itemLevel = itemLevel,
                equipPosition = equipPosition,
            };
        }
    }
}
