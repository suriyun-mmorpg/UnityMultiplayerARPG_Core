using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponItemEquipType : byte
{
    OneHand,
    OneHandCanDual,
    TwoHand,
}

[CreateAssetMenu(fileName = "WeaponItem", menuName = "Create GameData/WeaponItem")]
public class WeaponItem : EquipmentItem
{
    public float attackRange;
    public DamageEntity damageEntityPrefab;
    public DamageAmount[] damages;
    public DamageEffectivenessAttribute[] effectivenessAttributes;
    public WeaponItemEquipType equipType;

    public DamageEntity DamageEntityPrefab
    {
        get
        {
            if (damageEntityPrefab == null)
                return GameInstance.Singleton.damageEntityPrefab;
            return damageEntityPrefab;
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        // Weapon equipment cannot set custom equip position
        equipPosition = string.Empty;
        base.OnValidate();
    }
#endif
}
