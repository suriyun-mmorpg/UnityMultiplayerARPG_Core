using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageElement", menuName = "Create GameData/DamageElement")]
public class DamageElement : BaseGameData
{
    public Resistance resistance;
    public float GetAdjustedDamageReceives(ICharacterData characterData, float damageAmount)
    {
        if (resistance == null)
            return damageAmount -= characterData.GetArmor();
        var resistances = characterData.GetResistancesWithBuffs();
        float resistanceAmount = 0f;
        resistances.TryGetValue(resistance, out resistanceAmount);
        if (resistanceAmount > resistance.maxAmount)
            resistanceAmount = resistance.maxAmount;
        return damageAmount -= damageAmount * resistanceAmount;
    }
}
