using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseDamageEntity : MonoBehaviour
    {
        protected IAttackerEntity attacker;
        protected CharacterItem weapon;
        protected Dictionary<DamageElement, MinMaxFloat> damageAmounts;
        protected CharacterBuff debuff;
        protected BaseSkill skill;
        protected short skillLevel;

        public GameInstance gameInstance
        {
            get { return GameInstance.Singleton; }
        }

        public BaseGameplayRule gameplayRule
        {
            get { return gameInstance.GameplayRule; }
        }

        public BaseGameNetworkManager gameManager
        {
            get { return BaseGameNetworkManager.Singleton; }
        }

        public bool IsServer
        {
            get { return gameManager.IsServer; }
        }

        public bool IsClient
        {
            get { return gameManager.IsClient; }
        }

        private Transform cacheTransform;
        public Transform CacheTransform
        {
            get
            {
                if (cacheTransform == null)
                    cacheTransform = GetComponent<Transform>();
                return cacheTransform;
            }
        }

        public virtual void Setup(
            IAttackerEntity attacker,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            CharacterBuff debuff,
            BaseSkill skill,
            short skillLevel)
        {
            this.attacker = attacker;
            this.weapon = weapon;
            this.damageAmounts = damageAmounts;
            this.debuff = debuff;
            this.skill = skill;
            this.skillLevel = skillLevel;
        }

        public virtual void ApplyDamageTo(IDamageableEntity target)
        {
            if (target == null)
                return;
            if (IsServer)
                target.ReceiveDamage(attacker, weapon, damageAmounts, debuff);
            if (IsClient)
                target.PlayHitEffects(damageAmounts.Keys, skill);
        }
    }
}
