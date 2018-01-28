using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : EquipmentItem
{
    public DamageAmount damage;
    public DamageEffectivenessAttribute[] effectivenessAttributes;
    public bool requireTwoHand;
    public bool canWieldDual;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (requireTwoHand && canWieldDual)
        {
            Debug.LogWarning("Weapon which require two hand cannot wield dual");
            canWieldDual = false;
        }
        base.OnValidate();
    }
#endif
}
