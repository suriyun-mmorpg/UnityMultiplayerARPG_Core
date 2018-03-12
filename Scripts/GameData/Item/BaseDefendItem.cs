using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseDefendItem : BaseEquipmentItem
{
    [Header("Armor")]
    public float baseArmor;
    public float armorIncreaseEachLevel;
}
