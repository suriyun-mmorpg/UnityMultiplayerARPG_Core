using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Item Refine Info", menuName = "Create GameData/Item Refine Info")]
    public partial class ItemRefine : BaseGameData
    {
        public ItemRefineLevel[] levels;
    }

    [System.Serializable]
    public class ItemRefineLevel
    {
        [Range(0.01f, 1f)]
        [SerializeField]
        private float successRate = 1;
        [SerializeField]
        private ItemAmount[] requireItems;
        [SerializeField]
        private int requireGold;
        [Tooltip("How many levels it will be decreased if refining failed")]
        [SerializeField]
        private short refineFailDecreaseLevels;
        [Tooltip("It will be destroyed if this value is TRUE and refining failed")]
        [SerializeField]
        private bool refineFailDestroyItem;

        public float SuccessRate { get { return successRate; } }
        private Dictionary<Item, short> cacheRequireItems;
        public Dictionary<Item, short> RequireItems
        {
            get
            {
                if (cacheRequireItems == null)
                    cacheRequireItems = GameDataHelpers.MakeItemAmountsDictionary(requireItems, new Dictionary<Item, short>());
                return cacheRequireItems;
            }
        }
        public float RequireGold { get { return requireGold; } }
        public short RefineFailDecreaseLevels { get { return refineFailDecreaseLevels; } }
        public bool RefineFailDestroyItem { get { return refineFailDestroyItem; } }


        public bool CanRefine(IPlayerCharacterData character)
        {
            if (character.Gold < requireGold)
                return false;
            if (requireItems == null || requireItems.Length == 0)
                return true;
            foreach (var requireItem in requireItems)
            {
                if (requireItem.item != null && character.CountNonEquipItems(requireItem.item.DataId) < requireItem.amount)
                    return false;
            }
            return true;
        }

        public bool RefineItem(IPlayerCharacterData character, int nonEquipIndex)
        {
            var isSuccess = false;
            var refiningItem = character.NonEquipItems[nonEquipIndex];
            if (Random.value <= successRate)
            {
                ++refiningItem.level;
                character.NonEquipItems[nonEquipIndex] = refiningItem;
                isSuccess = true;
            }
            else
            {
                if (refineFailDestroyItem)
                    character.NonEquipItems.RemoveAt(nonEquipIndex);
                else
                {
                    refiningItem.level -= refineFailDecreaseLevels;
                    if (refiningItem.level < 1)
                        refiningItem.level = 1;
                    character.NonEquipItems[nonEquipIndex] = refiningItem;
                }
            }
            foreach (var requireItem in requireItems)
            {
                if (requireItem.item != null && requireItem.amount > 0)
                    character.DecreaseItems(requireItem.item.DataId, requireItem.amount);
            }
            character.Gold -= requireGold;
            return isSuccess;
        }
    }
}
