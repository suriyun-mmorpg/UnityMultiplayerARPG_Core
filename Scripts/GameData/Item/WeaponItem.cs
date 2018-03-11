using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItem", menuName = "Create GameData/WeaponItem")]
public class WeaponItem : BaseEquipmentItem
{
    [Header("Weapon Data")]
    public WeaponType weaponType;
    public DamageAttribute damageAttribute;

    public WeaponType WeaponType
    {
        get
        {
            if (weaponType == null)
            {
                weaponType = CreateInstance<WeaponType>();
                weaponType.effectivenessAttributes = new DamageEffectivenessAttribute[0];
                weaponType.rightHandAttackAnimations = new ActionAnimation[0];
                weaponType.leftHandAttackAnimations = new ActionAnimation[0];
                weaponType.damage = new Damage();
            }
            return weaponType;
        }
    }
}
