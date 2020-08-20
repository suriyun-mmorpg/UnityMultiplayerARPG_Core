using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseDamageEntity : PoolDescriptor, IPoolDescriptor
    {
        protected IGameEntity attacker;
        protected CharacterItem weapon;
        protected Dictionary<DamageElement, MinMaxFloat> damageAmounts;
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

        protected ParticleSystem[] particles;
        protected AudioSource[] audioSources;
        protected AudioSourceSetter[] audioSourceSetters;

        protected virtual void Awake()
        {
            CacheTransform = transform;
            particles = GetComponentsInChildren<ParticleSystem>(true);
            audioSources = GetComponentsInChildren<AudioSource>(true);
            audioSourceSetters = GetComponentsInChildren<AudioSourceSetter>(true);
        }

        public virtual void Setup(
            IGameEntity attacker,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel)
        {
            this.attacker = attacker;
            this.weapon = weapon;
            this.damageAmounts = damageAmounts;
            this.skill = skill;
            this.skillLevel = skillLevel;
        }

        public virtual void ApplyDamageTo(IDamageableEntity target)
        {
            if (target == null || target.IsDead() || !target.CanReceiveDamageFrom(attacker))
                return;
            if (IsClient)
                target.PlayHitEffects(damageAmounts.Keys, skill);
            if (IsServer)
                target.ReceiveDamage(attacker, damageAmounts, weapon, skill, skillLevel);
        }

        public override void InitPrefab()
        {
            if (this == null)
            {
                Debug.LogWarning("The Base Damage Entity is null, this should not happens " + this);
                return;
            }
            // Prepare audio sources
            audioSources = GetComponentsInChildren<AudioSource>(true);
            if (audioSources != null && audioSources.Length > 0)
            {
                foreach (AudioSource audioSource in audioSources)
                {
                    if (!audioSource)
                        continue;
                    audioSource.playOnAwake = false;
                }
            }
            // Prepare audio source setters
            audioSourceSetters = GetComponentsInChildren<AudioSourceSetter>(true);
            if (audioSourceSetters != null && audioSourceSetters.Length > 0)
            {
                foreach (AudioSourceSetter audioSourceSetter in audioSourceSetters)
                {
                    if (!audioSourceSetter)
                        continue;
                    audioSourceSetter.playOnAwake = false;
                    audioSourceSetter.playOnEnable = false;
                }
            }
            base.InitPrefab();
        }

        public override void OnGetInstance()
        {
            // Play particles
            if (particles != null && particles.Length > 0)
            {
                foreach (ParticleSystem particle in particles)
                {
                    if (!particle)
                        continue;
                    particle.Play();
                }
            }
            // Play audio sources
            if (audioSourceSetters != null && audioSourceSetters.Length > 0)
            {
                foreach (AudioSourceSetter audioSourceSetter in audioSourceSetters)
                {
                    if (!audioSourceSetter)
                        continue;
                    audioSourceSetter.Play();
                }
            }
            if (audioSources != null && audioSources.Length > 0)
            {
                float volume = AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level;
                foreach (AudioSource audioSource in audioSources)
                {
                    if (!audioSource)
                        continue;
                    audioSource.volume = volume;
                    audioSource.Play();
                }
            }
            base.OnGetInstance();
        }
    }
}
