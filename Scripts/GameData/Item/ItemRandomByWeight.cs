using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemRandomByWeight : IGameDataValidation
    {
#if UNITY_EDITOR
        public BaseItem item;
#endif

        [ReadOnlyField]
        public int dataId;
        [Tooltip("Set `minAmount` to <= `0` to not random amount, it will use `maxAmount` as a randomed amount")]
        public int minAmount;
        [FormerlySerializedAs("amount")]
        [Min(1)]
        public int maxAmount;
        public int randomWeight;
        public BaseItem Item
        {
            get
            {
                if (GameInstance.Items.TryGetValue(dataId, out BaseItem item))
                    return item;
                return null;
            }
        }

        public bool OnValidateGameData()
        {
#if UNITY_EDITOR
            int newDataId = item != null ? item.DataId : 0;
            if (dataId != newDataId)
            {
                Debug.Log($"[ItemRandomByWeight] Set `dataId` from {dataId} to {newDataId}");
                dataId = newDataId;
                return true;
            }
#endif
            return false;
        }
    }
}
