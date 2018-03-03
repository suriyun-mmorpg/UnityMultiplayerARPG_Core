using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponItemEquipType : byte
{
    OneHand,
    OneHandCanDual,
    TwoHand,
}

[System.Serializable]
public class WeaponAttackAnimation
{
    public int actionId;
    public float damageDuration;
    public float totalDuration;
}

[CreateAssetMenu(fileName = "WeaponItem", menuName = "Create GameData/WeaponItem")]
public class WeaponType : BaseGameData
{
    public float attackRange = 1f;
    public WeaponItemEquipType equipType = WeaponItemEquipType.OneHand;
    public DamageEffectivenessAttribute[] effectivenessAttributes;
    public WeaponAttackAnimation[] mainAttackAnimations;
    public WeaponAttackAnimation[] subAttackAnimations;
    public DamageEntity damageEntityPrefab;
}
