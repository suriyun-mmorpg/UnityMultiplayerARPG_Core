using System.Collections.Generic;
using UnityEngine;

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

        [Header("Cash Shop Generating Configs")]
        [SerializeField]
        protected CashShopItemGeneratingData[] cashShopItemGeneratingList;

        public override string Title
        {
            get
            {
                if (itemRefine == null || itemRefine.titleColor.a == 0)
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

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GenerateCashShopItems();
        }

        public void GenerateCashShopItems()
        {
            if (cashShopItemGeneratingList == null || cashShopItemGeneratingList.Length == 0)
                return;

            List<string> languageKeys = new List<string>(LanguageManager.Languages.Keys);
            CashShopItemGeneratingData generatingData;
            CashShopItem cashShopItem;
            List<LanguageData> titleLanguageDataList = new List<LanguageData>();
            List<LanguageData> descriptionLanguageDataList = new List<LanguageData>();
            for (int i = 0; i < cashShopItemGeneratingList.Length; ++i)
            {
                generatingData = cashShopItemGeneratingList[i];
                cashShopItem = CreateInstance<CashShopItem>();
                cashShopItem.name = $"<CASHSHOPITEM_{name}_{i}>";
                cashShopItem.title = string.Format(LanguageManager.GetText(UIFormatKeys.UI_FORMAT_GENERATE_CAST_SHOP_ITEM_TITLE.ToString()), title, generatingData.amount);
                cashShopItem.description = string.Format(LanguageManager.GetText(UIFormatKeys.UI_FORMAT_GENERATE_CAST_SHOP_ITEM_DESCRIPTION.ToString()), title, generatingData.amount, description);
                titleLanguageDataList.Clear();
                descriptionLanguageDataList.Clear();
                foreach (string languageKey in languageKeys)
                {
                    titleLanguageDataList.Add(new LanguageData()
                    {
                        key = languageKey,
                        value = string.Format(LanguageManager.GetTextByLanguage(languageKey, UIFormatKeys.UI_FORMAT_GENERATE_CAST_SHOP_ITEM_TITLE.ToString()), Language.GetTextByLanguageKey(titles, languageKey, title), generatingData.amount),
                    });
                    descriptionLanguageDataList.Add(new LanguageData()
                    {
                        key = languageKey,
                        value = string.Format(LanguageManager.GetTextByLanguage(languageKey, UIFormatKeys.UI_FORMAT_GENERATE_CAST_SHOP_ITEM_DESCRIPTION.ToString()), Language.GetTextByLanguageKey(titles, languageKey, title), generatingData.amount, Language.GetTextByLanguageKey(descriptions, languageKey, description)),
                    });
                }
                cashShopItem.titles = titleLanguageDataList.ToArray();
                cashShopItem.descriptions = descriptionLanguageDataList.ToArray();
                cashShopItem.category = category;
                cashShopItem.icon = icon;
                cashShopItem.sellPriceCash = generatingData.sellPriceCash;
                cashShopItem.sellPriceGold = generatingData.sellPriceGold;
                cashShopItem.receiveItems = new ItemAmount[]
                {
                    new ItemAmount()
                    {
                        item = this,
                        amount = generatingData.amount,
                    }
                };
            }
        }
    }
}
