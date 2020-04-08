using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseDamageEntity : MonoBehaviour, IPoolDescriptor
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
        
        [SerializeField]
        private int poolSize = 30;
        public int PoolSize
        {
            get { return poolSize; }
        }

        public IPoolDescriptor ObjectPrefab
        {
            get; set;
        }

        public Transform CacheTransform { get; private set; }

        private ParticleSystem[] particles;
        private AudioSource[] audioSources;
        private AudioSourceSetter[] audioSourceSetters;

        protected virtual void Awake()
        {
            CacheTransform = transform;
            particles = GetComponentsInChildren<ParticleSystem>();
            audioSources = GetComponentsInChildren<AudioSource>();
            audioSourceSetters = GetComponentsInChildren<AudioSourceSetter>();
        }

        protected virtual void PushBack(float delay)
        {
            Invoke("PushBack", delay);
        }

        protected virtual void PushBack()
        {
            OnPushBack();
            PoolSystem.PushBack(this);
        }

        protected virtual void OnPushBack()
        {

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
                target.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);
        }

        public virtual void InitPrefab()
        {
            // Prepare audio sources
            if (audioSources == null)
                audioSources = GetComponentsInChildren<AudioSource>();
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
            if (audioSourceSetters == null)
                audioSourceSetters = GetComponentsInChildren<AudioSourceSetter>();
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
        }

        public virtual void OnGetInstance()
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
        }
    }
}
