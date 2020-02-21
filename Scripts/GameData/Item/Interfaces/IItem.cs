using UnityEngine;

namespace MultiplayerARPG
{
    public partial interface IItem
    {
        int DataId { get; }
        ItemType ItemType { get; }
        GameObject DropModel { get; }
        int SellPrice { get; }
        float Weight { get; }
        short MaxStack { get; }
        ItemRefine ItemRefine { get; }
        int MaxLevel { get; }
        float LockDuration { get; }
        int DismantleReturnGold { get; }
        ItemAmount[] DismantleReturnItems { get; }

        bool IsEquipment();
        bool IsUsable();
        bool IsDefendEquipment();
        bool IsArmor();
        bool IsShield();
        bool IsWeapon();
        bool IsPotion();
        bool IsAmmo();
        bool IsBuilding();
        bool IsPet();
        bool IsSocketEnhancer();
        bool IsMount();
        bool IsSkill();
    }
}
