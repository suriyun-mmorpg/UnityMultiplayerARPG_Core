using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArmorItem", menuName = "Create GameData/ArmorItem")]
public class ArmorItem : BaseDefendItem
{
    [Header("Equip Data")]
    public ArmorType armorType;

    public ArmorType ArmorType
    {
        get
        {
            if (armorType == null)
                armorType = GameInstance.Singleton.DefaultArmorType;
            return armorType;
        }
    }

    public string EquipPosition
    {
        get { return ArmorType.Id; }
    }
}
