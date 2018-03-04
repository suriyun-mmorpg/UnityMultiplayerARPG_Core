using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItem", menuName = "Create GameData/WeaponItem")]
public class WeaponItem : EquipmentItem
{
    public WeaponType weaponType;
    public DamageAmount[] damageAmounts;

    public WeaponType WeaponType
    {
        get
        {
            if (weaponType == null)
            {
                weaponType = CreateInstance<WeaponType>();
                weaponType.effectivenessAttributes = new DamageEffectivenessAttribute[0];
                weaponType.mainAttackAnimations = new WeaponAttackAnimation[0];
                weaponType.subAttackAnimations = new WeaponAttackAnimation[0];
            }
            return weaponType;
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
                    var id = damageAmount.damageElement == null ? GameDataConst.DEFAULT_DAMAGE_ID : damageAmount.damageElement.Id;
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
