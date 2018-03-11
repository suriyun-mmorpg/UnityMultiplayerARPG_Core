using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShieldItem", menuName = "Create GameData/ShieldItem")]
public class ShieldItem : BaseEquipmentItem
{
    [Header("Armor")]
    public float baseArmor;
    public float armorIncreaseEachLevel;
}
