using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseItem : BaseGameData, IItem
    {
        [Category("Item Settings")]
        [SerializeField]
        [Min(0)]
        protected int sellPrice = 0;
        [SerializeField]
        [Min(0f)]
        protected float weight = 0f;
        [SerializeField]
        [Min(1)]
        protected int maxStack = 1;
        [SerializeField]
        protected ItemRefine itemRefine = null;
        [SerializeField]
        [Tooltip("This is duration to lock item at first time when pick up dropped item or bought it from NPC or IAP system")]
        protected float lockDuration = 0;
        [SerializeField]
        [Tooltip("This is duration to make item to be expired and destroyed from inventory, set it to 0 to not apply expiring duration, set it to 7 to make it expire in next 7 hours")]
        protected int expireDuration = 0;

        [Category(10, "In-Scene Objects/Appearance")]
        [SerializeField]
        protected GameObject dropModel = null;

        [Category(50, "Dismantle Settings")]
        [SerializeField]
        protected int dismantleReturnGold = 0;
        [SerializeField]
        protected ItemAmount[] dismantleReturnItems = new ItemAmount[0];
        [SerializeField]
        protected CurrencyAmount[] dismantleReturnCurrencies = new CurrencyAmount[0];

        [Category(100, "Cash Shop Generating Settings")]
        [SerializeField]
        protected CashShopItemGeneratingData[] cashShopItemGeneratingList = new CashShopItemGeneratingData[0];

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

        public int MaxStack { get { return maxStack; } }

        public ItemRefine ItemRefine { get { return itemRefine; } }

        public float LockDuration { get { return lockDuration; } }

        public int ExpireDuration { get { return expireDuration; } }

        public int DismantleReturnGold { get { return dismantleReturnGold; } }

        public ItemAmount[] DismantleReturnItems { get { return dismantleReturnItems; } }

        public CurrencyAmount[] DismantleReturnCurrencies { get { return dismantleReturnCurrencies; } }

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
            if (ItemRefine != null)
                ItemRefine.PrepareRelatesData();
            GameInstance.AddItems(DismantleReturnItems);
            GameInstance.AddCurrencies(DismantleReturnCurrencies);
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
                GameInstance.CashShopItems[cashShopItem.DataId] = cashShopItem;
            }
        }
    }
}
