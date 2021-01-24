using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class BaseBuffEntity : MonoBehaviour, IPoolDescriptor
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
            particles = GetComponentsInChildren<ParticleSystem>(true);
            audioSources = GetComponentsInChildren<AudioSource>(true);
            audioSourceSetters = GetComponentsInChildren<AudioSourceSetter>(true);
        }

        protected virtual void PushBack(float delay)
        {
            Invoke(nameof(PushBack), delay);
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
            if (target == null || (!applyBuffToEveryone && !target.IsAlly(buffApplier.GetInfo())))
                return;
            target.ApplyBuff(skill.DataId, BuffType.SkillBuff, skillLevel, buffApplier.GetInfo());
        }

        public virtual void InitPrefab()
        {
            if (this == null)
            {
                Debug.LogWarning("The Base Bufff Entity is null, this should not happens " + this);
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
