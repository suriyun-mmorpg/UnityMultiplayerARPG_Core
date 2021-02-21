using UnityEngine;

namespace MultiplayerARPG
{

    [CreateAssetMenu(fileName = "Item Craft Formula", menuName = "Create GameData/Item Craft Formula", order = -4880)]
    public class ItemCraftFormula : ScriptableObject, IGameData
    {
        public virtual string Id { get { return name; } }

        public int DataId { get { return Id.GenerateHashId(); } }

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

        public void PrepareRelatesData()
        {

        }

        public bool Validate()
        {
            return false;
        }
    }
}
