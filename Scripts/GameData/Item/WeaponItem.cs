using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponItemEquipType : byte
{
    OneHand,
    OneHandCanDual,
    TwoHand,
}

public class WeaponItem : EquipmentItem
{
    public DamageAmount damage;
    public DamageEffectivenessAttribute[] effectivenessAttributes;
    public WeaponItemEquipType equipType;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        // Weapon equipment cannot set custom equip position
        equipPosition = string.Empty;
        base.OnValidate();
    }
#endif
}
