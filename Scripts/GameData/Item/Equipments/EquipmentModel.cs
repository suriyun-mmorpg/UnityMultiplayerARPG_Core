using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct EquipmentModel
    {
        public string equipSocket;
        public bool useInstantiatedObject;
        [BoolShowConditional(nameof(useInstantiatedObject), false)]
        public GameObject model;
        [BoolShowConditional(nameof(useInstantiatedObject), true)]
        public int instantiatedObjectIndex;
    }
}
