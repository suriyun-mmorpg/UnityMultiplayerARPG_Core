using UnityEngine;
using System.Collections;

namespace MultiplayerARPG
{
    public abstract partial class BaseItem : BaseGameData, IItem
    {
        [Header("Item Configs")]
        [SerializeField]
        protected GameObject dropModel;
        [SerializeField]
        protected int sellPrice;
        [SerializeField]
        protected float weight;
        [SerializeField]
        [Range(1, 1000)]
        protected short maxStack = 1;
        [SerializeField]
        protected ItemRefine itemRefine;
        [SerializeField]
        [Tooltip("This is duration to lock item at first time when pick up dropped item or bought it from NPC or IAP system")]
        protected float lockDuration;

        [Header("Dismantle Configs")]
        [SerializeField]
        protected int dismantleReturnGold;
        [SerializeField]
        protected ItemAmount[] dismantleReturnItems;

        public override string Title
        {
            get
            {
                if (itemRefine == null)
                    return base.Title;
                return "<color=#" + ColorUtility.ToHtmlStringRGB(itemRefine.titleColor) + ">" + base.Title + "</color>";
            }
        }

        public virtual string RarityTitle
        {
            get
            {
                if (itemRefine == null)
                    return "Normal";
                return "<color=#" + ColorUtility.ToHtmlStringRGB(itemRefine.titleColor) + ">" + itemRefine.Title + "</color>";
            }
        }

        public virtual string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_JUNK.ToString()); }
        }

        public virtual ItemType ItemType { get { return ItemType.Junk; } }

        public GameObject DropModel { get { return dropModel; } }

        public int SellPrice { get { return sellPrice; } }

        public float Weight { get { return weight; } }

        public short MaxStack { get { return maxStack; } }

        public ItemRefine ItemRefine { get { return itemRefine; } }

        public float LockDuration { get { return lockDuration; } }

        public int DismantleReturnGold { get { return dismantleReturnGold; } }

        public ItemAmount[] DismantleReturnItems { get { return dismantleReturnItems; } }

        public int MaxLevel
        {
            get
            {
                if (!ItemRefine || ItemRefine.levels == null || ItemRefine.levels.Length == 0)
                    return 1;
                return ItemRefine.levels.Length;
            }
        }

        public override bool Validate()
        {
            bool hasChanges = false;
            // Equipment / Pet max stack always equals to 1
            switch (ItemType)
            {
                case ItemType.Armor:
                case ItemType.Weapon:
                case ItemType.Shield:
                case ItemType.Pet:
                case ItemType.Mount:
                    if (maxStack != 1)
                    {
                        maxStack = 1;
                        hasChanges = true;
                    }
                    break;
            }
            return hasChanges;
        }
    }
}
