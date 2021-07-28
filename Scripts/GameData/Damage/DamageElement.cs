using System.Collections.Generic;
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
        public GameEffect[] DamageHitEffects
        {
            get { return damageHitEffects; }
        }

        public float GetDamageReducedByResistance(Dictionary<DamageElement, float> damageReceiverResistances, Dictionary<DamageElement, float> damageReceiverArmors, float damageAmount)
        {
            return GameInstance.Singleton.GameplayRule.GetDamageReducedByResistance(damageReceiverResistances, damageReceiverArmors, damageAmount, this);
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddPoolingObjects(damageHitEffects);
        }
    }
}
