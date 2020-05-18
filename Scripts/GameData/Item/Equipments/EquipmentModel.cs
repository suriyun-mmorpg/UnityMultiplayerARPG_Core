using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct EquipmentModel
    {
        public string equipSocket;
        public bool useInstantiatedObject;
        [BoolShowConditional(conditionFieldName: "useInstantiatedObject", conditionValue: false)]
        public GameObject model;
        [BoolShowConditional(conditionFieldName: "useInstantiatedObject", conditionValue: true)]
        public int instantiatedObjectIndex;
    }
}
