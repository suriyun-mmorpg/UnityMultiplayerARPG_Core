using System;
using UnityEngine;

namespace MultiplayerARPG
{

    [CreateAssetMenu(fileName = "Item Craft Formula", menuName = "Create GameData/Item Craft Formula", order = -4880)]
    public class ItemCraftFormula : BaseGameData
    {
        [Category("Item Craft Formula Settings")]
        [SerializeField]
        private ItemCraft itemCraft = default(ItemCraft);
        public ItemCraft ItemCraft
        {
            get { return itemCraft; }
        }

        [SerializeField]
        private float craftDuration = 0f;
        public float CraftDuration
        {
            get { return craftDuration; }
        }

        public int SourceId { get; set; }
    }
}
