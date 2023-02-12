using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct EquipmentModel
    {
        [Header("Generic Settings")]
        public string equipSocket;
        public bool useInstantiatedObject;
        [BoolShowConditional(nameof(useInstantiatedObject), false)]
        public GameObject model;
        [BoolShowConditional(nameof(useInstantiatedObject), true)]
        public int instantiatedObjectIndex;
        public byte priority;

        [Header("Transform Settings")]
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
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
                model = model,
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
