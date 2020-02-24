using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class BaseBuffEntity : MonoBehaviour
    {
        [Tooltip("If this is `TRUE` buffs will applies to everyone including with an enemies")]
        public bool applyBuffToEveryone;

        protected BaseCharacterEntity buffApplier;
        protected BaseSkill skill;
        protected short skillLevel;

        public GameInstance CurrentGameInstance
        {
            get { return GameInstance.Singleton; }
        }

        public BaseGameplayRule CurrentGameplayRule
        {
            get { return CurrentGameInstance.GameplayRule; }
        }

        public BaseGameNetworkManager CurrentGameManager
        {
            get { return BaseGameNetworkManager.Singleton; }
        }

        public bool IsServer
        {
            get { return CurrentGameManager.IsServer; }
        }

        public bool IsClient
        {
            get { return CurrentGameManager.IsClient; }
        }
        
        public Transform CacheTransform { get; private set; }

        private void Awake()
        {
            CacheTransform = transform;
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
            if (target == null || (!applyBuffToEveryone && !target.IsAlly(buffApplier)))
                return;
            target.ApplyBuff(skill.DataId, BuffType.SkillBuff, skillLevel, buffApplier);
        }
    }
}
