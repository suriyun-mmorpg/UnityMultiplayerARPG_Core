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

        public virtual bool IsDefendEquipment()
        {
            return IsArmor() || IsShield();
        }

        public virtual bool IsEquipment()
        {
            return IsDefendEquipment() || IsWeapon();
        }

        public virtual bool IsUsable()
        {
            return IsPotion() || IsPet() || IsMount() || IsSkill();
        }

        public virtual bool IsArmor()
        {
            return ItemType == ItemType.Armor;
        }

        public virtual bool IsShield()
        {
            return ItemType == ItemType.Shield;
        }

        public virtual bool IsWeapon()
        {
            return ItemType == ItemType.Weapon;
        }

        public virtual bool IsPotion()
        {
            return ItemType == ItemType.Potion;
        }

        public virtual bool IsAmmo()
        {
            return ItemType == ItemType.Ammo;
        }

        public virtual bool IsBuilding()
        {
            return ItemType == ItemType.Building;
        }

        public virtual bool IsPet()
        {
            return ItemType == ItemType.Pet;
        }

        public virtual bool IsSocketEnhancer()
        {
            return ItemType == ItemType.SocketEnhancer;
        }

        public virtual bool IsMount()
        {
            return ItemType == ItemType.Mount;
        }

        public virtual bool IsSkill()
        {
            return ItemType == ItemType.Skill;
        }
    }
}
