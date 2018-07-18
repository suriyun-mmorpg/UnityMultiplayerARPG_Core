using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "DamageElement", menuName = "Create GameData/DamageElement")]
    public partial class DamageElement : BaseGameData
    {
        [Range(0f, 1f)]
        public float maxResistanceAmount;
        public GameEffectCollection hitEffects;

        public float GetDamageReducedByResistance(BaseCharacterEntity damageReceiver, float damageAmount)
        {
            return gameInstance.GameplayRule.GetDamageReducedByResistance(damageReceiver, damageAmount, this);
        }
    }
}
