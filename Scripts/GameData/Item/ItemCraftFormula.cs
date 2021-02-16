using UnityEngine;

namespace MultiplayerARPG
{
    public class ItemCraftFormula : BaseGameData
    {
        [SerializeField]
        private ItemCraft itemCraft;

        [SerializeField]
        private float craftDuration;

        public ItemCraft ItemCraft
        {
            get { return itemCraft; }
        }

        public float CraftDuration
        {
            get { return craftDuration; }
        }
    }
}
