using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseDamageEntity : MonoBehaviour
    {
        public string Id { get { return name; } }
        public int DataId { get { return BaseGameData.MakeDataId(Id); } }

        protected IAttackerEntity attacker;
        protected CharacterItem weapon;
        protected Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;
        protected CharacterBuff debuff;
        protected Skill skill;

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
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            CharacterBuff debuff,
            Skill skill)
        {
            this.attacker = attacker;
            this.weapon = weapon;
            this.allDamageAmounts = allDamageAmounts;
            this.debuff = debuff;
            this.skill = skill;
        }

        public virtual void ApplyDamageTo(IDamageableEntity target)
        {
            if (target == null)
                return;
            if (IsServer)
                target.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff);
            if (IsClient)
                target.PlayHitEffects(allDamageAmounts.Keys, skill);
        }
    }
}
