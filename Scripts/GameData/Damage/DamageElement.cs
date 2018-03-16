using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageElement", menuName = "Create GameData/DamageElement")]
public class DamageElement : BaseGameData
{
    public Resistance resistance;
    public float GetDamageReducedByResistance(ICharacterData damageReceiver, float damageAmount)
    {
        var gameInstance = GameInstance.Singleton;
        return gameInstance.GameplayRule.GetDamageReducedByResistance(damageReceiver, damageAmount, resistance);
    }
}
