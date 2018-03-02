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
    public DamageAmount[] damageAmounts;
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

    private Dictionary<string, DamageAmount> tempDamageAmounts;
    public Dictionary<string, DamageAmount> TempDamageAmounts
    {
        get
        {
            if (tempDamageAmounts == null)
            {
                tempDamageAmounts = new Dictionary<string, DamageAmount>();
                foreach (var damageAmount in damageAmounts)
                {
                    var id = damageAmount.damage == null ? GameDataConst.DEFAULT_DAMAGE_ID : damageAmount.damage.Id;
                    tempDamageAmounts[id] = damageAmount;
                }
            }
            return tempDamageAmounts;
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        // Damage Amounts must have at least 1
        if (damageAmounts == null || damageAmounts.Length == 0)
            damageAmounts = new DamageAmount[] { new DamageAmount() };
        // Weapon equipment cannot set custom equip position
        equipPosition = string.Empty;
        base.OnValidate();
    }
#endif
}
