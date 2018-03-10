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
public class WeaponType : BaseGameData
{
    public WeaponItemEquipType equipType = WeaponItemEquipType.OneHand;
    public DamageEffectivenessAttribute[] effectivenessAttributes;
    public ActionAnimation[] mainAttackAnimations;
    public ActionAnimation[] subAttackAnimations;
    public Damage damage;

    private Dictionary<Attribute, float> cacheEffectivenessAttributes;
    public Dictionary<Attribute, float> CacheEffectivenessAttributes
    {
        get
        {
            if (cacheEffectivenessAttributes == null)
                cacheEffectivenessAttributes = GameDataHelpers.MakeDamageEffectivenessAttributesDictionary(effectivenessAttributes, new Dictionary<Attribute, float>());
            return cacheEffectivenessAttributes;
        }
    }
}
