using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShieldItem", menuName = "Create GameData/ShieldItem")]
public class ShieldItem : EquipmentItem
{
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        // Shield equipment cannot set custom equip position
        equipPosition = string.Empty;
        base.OnValidate();
    }
#endif
}
