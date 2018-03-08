using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItem", menuName = "Create GameData/WeaponItem")]
public class WeaponItem : EquipmentItem
{
    public WeaponType weaponType;
    public DamageAttribute baseDamageAttribute;
    public DamageAttribute[] additionalDamageAttributes;

    public WeaponType WeaponType
    {
        get
        {
            if (weaponType == null)
            {
                weaponType = CreateInstance<WeaponType>();
                weaponType.effectivenessAttributes = new DamageEffectivenessAttribute[0];
                weaponType.mainAttackAnimations = new ActionAnimation[0];
                weaponType.subAttackAnimations = new ActionAnimation[0];
                weaponType.damage = new Damage();
            }
            return weaponType;
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        // Damage Amounts must have at least 1
        if (additionalDamageAttributes == null || additionalDamageAttributes.Length == 0)
            additionalDamageAttributes = new DamageAttribute[] { new DamageAttribute() };
        // Weapon equipment cannot set custom equip position
        equipPosition = string.Empty;
        base.OnValidate();
    }
#endif
}
