using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Damage Element", menuName = "Create GameData/Damage Element", order = -4994)]
    public partial class DamageElement : BaseGameData
    {
        [Header("Damage Element Configs")]
        [Range(0f, 1f)]
        public float maxResistanceAmount;
        public GameEffect[] damageHitEffects;

        public float GetDamageReducedByResistance(BaseCharacterEntity damageReceiver, float damageAmount)
        {
            return GameInstance.Singleton.GameplayRule.GetDamageReducedByResistance(damageReceiver, damageAmount, this);
        }

        public GameEffect[] GetDamageHitEffects()
        {
            return damageHitEffects;
        }
    }
}
