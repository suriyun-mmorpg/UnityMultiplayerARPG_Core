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
                weaponType = GameInstance.Singleton.DefaultWeaponType;
            return weaponType;
        }
    }

    public WeaponItemEquipType EquipType
    {
        get { return WeaponType.equipType; }
    }
}
