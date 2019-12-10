using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class BaseBuffEntity : MonoBehaviour
    {
        protected BaseCharacterEntity buffApplier;
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
            BaseCharacterEntity buffApplier,
            BaseSkill skill,
            short skillLevel)
        {
            this.buffApplier = buffApplier;
            this.skill = skill;
            this.skillLevel = skillLevel;
        }

        public virtual void ApplyBuffTo(BaseCharacterEntity target)
        {
            if (target == null)
                return;
            target.ApplyBuff(skill.DataId, BuffType.SkillBuff, skillLevel, buffApplier);
        }
    }
}
