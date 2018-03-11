using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponItemEquipType : byte
{
    OneHand,
    OneHandCanDual,
    TwoHand,
}

[CreateAssetMenu(fileName = "WeaponType", menuName = "Create GameData/WeaponType")]
public class WeaponType : BaseGameData
{
    public WeaponItemEquipType equipType = WeaponItemEquipType.OneHand;
    public DamageEffectivenessAttribute[] effectivenessAttributes;
    public ActionAnimation[] rightHandAttackAnimations;
    public ActionAnimation[] leftHandAttackAnimations;
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
