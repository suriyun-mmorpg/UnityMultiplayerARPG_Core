using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageElement", menuName = "Create GameData/DamageElement")]
public class DamageElement : BaseGameData
{
    [Range(0f, 1f)]
    public float maxResistanceAmount;
    public GameEffectCollection hitEffects;

    public float GetDamageReducedByResistance(BaseCharacterEntity damageReceiver, float damageAmount)
    {
        var gameInstance = GameInstance.Singleton;
        return gameInstance.GameplayRule.GetDamageReducedByResistance(damageReceiver, damageAmount, this);
    }
}
