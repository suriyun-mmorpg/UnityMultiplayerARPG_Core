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

    private Dictionary<string, DamageEffectivenessAttribute> tempEffectivenessAttributes;
    public Dictionary<string, DamageEffectivenessAttribute> TempEffectivenessAttributes
    {
        get
        {
            if (tempEffectivenessAttributes == null)
            {
                tempEffectivenessAttributes = new Dictionary<string, DamageEffectivenessAttribute>();
                foreach (var effectivenessAttribute in effectivenessAttributes)
                {
                    if (effectivenessAttribute.attribute == null)
                        continue;
                    tempEffectivenessAttributes[effectivenessAttribute.attribute.Id] = effectivenessAttribute;
                }
            }
            return tempEffectivenessAttributes;
        }
    }
}
