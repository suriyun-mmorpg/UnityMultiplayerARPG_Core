using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseItem : BaseGameData, IItem
    {
        [Category("Item Settings")]
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

        [Category(10, "In-Scene Objects/Appearance")]
        [SerializeField]
        protected GameObject dropModel;

        [Category(50, "Dismantle Settings")]
        [SerializeField]
        protected int dismantleReturnGold;
        [SerializeField]
        protected ItemAmount[] dismantleReturnItems;

        [Category(100, "Cash Shop Generating Settings")]
        [SerializeField]
        protected CashShopItemGeneratingData[] cashShopItemGeneratingList;

        public override string Title
        {
            get
            {
                if (itemRefine == null || itemRefine.TitleColor.a == 0)
                    return base.Title;
                return "<color=#" + ColorUtility.ToHtmlStringRGB(itemRefine.TitleColor) + ">" + base.Title + "</color>";
            }
        }

        public virtual string RarityTitle
        {
            get
            {
                if (itemRefine == null)
                    return "Normal";
                return "<color=#" + ColorUtility.ToHtmlStringRGB(itemRefine.TitleColor) + ">" + itemRefine.Title + "</color>";
            }
        }

        public abstract string TypeTitle { get; }

        public abstract ItemType ItemType { get; }

        public GameObject DropModel { get { return dropModel; } set { dropModel = value; } }

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
                if (!ItemRefine || ItemRefine.Levels == null || ItemRefine.Levels.Length == 0)
                    return 1;
                return ItemRefine.Levels.Length;
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

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GenerateCashShopItems();
        }

        public void GenerateCashShopItems()
        {
            if (cashShopItemGeneratingList == null || cashShopItemGeneratingList.Length == 0)
                return;

            CashShopItemGeneratingData generatingData;
            CashShopItem cashShopItem;
            for (int i = 0; i < cashShopItemGeneratingList.Length; ++i)
            {
                generatingData = cashShopItemGeneratingList[i];
                cashShopItem = CreateInstance<CashShopItem>();
                cashShopItem.name = $"<CASHSHOPITEM_{name}_{i}>";
                cashShopItem.GenerateByItem(this, generatingData);
            }
        }
    }
}
