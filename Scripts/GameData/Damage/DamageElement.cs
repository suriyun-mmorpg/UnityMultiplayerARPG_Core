using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.DAMAGE_ELEMENT_FILE, menuName = GameDataMenuConsts.DAMAGE_ELEMENT_MENU, order = GameDataMenuConsts.DAMAGE_ELEMENT_ORDER)]
    public partial class DamageElement : BaseGameData
    {
        [Category("Damage Element Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float maxResistanceAmount = 1f;
        public float MaxResistanceAmount { get { return maxResistanceAmount; } }

        [SerializeField]
        private GameEffect[] damageHitEffects = new GameEffect[0];
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

        public DamageElement GenerateDefaultDamageElement(GameEffect[] defaultDamageHitEffects)
        {
            name = GameDataConst.DEFAULT_DAMAGE_ID;
            defaultTitle = GameDataConst.DEFAULT_DAMAGE_TITLE;
            maxResistanceAmount = 1f;
            damageHitEffects = defaultDamageHitEffects;
            return this;
        }
    }
}
