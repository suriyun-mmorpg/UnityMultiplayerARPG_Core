using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArmorItem", menuName = "Create GameData/ArmorItem")]
public class ArmorItem : BaseEquipmentItem
{
    [Header("Equip Data")]
    public string equipPosition;

    [Header("Armor")]
    public float baseArmor;
    public float armorIncreaseEachLevel;
}
