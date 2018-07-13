using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "ItemRefineInfo", menuName = "Create GameData/ItemRefineInfo")]
    public class ItemRefineInfo : BaseGameData
    {
        public ItemRefine[] itemRefines;
    }

    [System.Serializable]
    public struct ItemRefine
    {
        [Range(0.01f, 1f)]
        public float successRate;
        public ItemAmount[] requireItems;
        public int refineFailDecreaseLevels;
        public bool refineFailDestroyItem;
    }
}
