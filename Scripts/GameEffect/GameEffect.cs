using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class GameEffect : PoolDescriptor, IPoolDescriptor
    {
        public string effectSocket;
        public bool isLoop;
        public float lifeTime;
        private bool intendToFollowingTarget;
        private Transform followingTarget;
        public Transform FollowingTarget
        {
            get { return followingTarget; }
            set
            {
                if (value == null)
                    return;
                followingTarget = value;
                intendToFollowingTarget = true;
            }
        }

        public Transform CacheTransform { get; private set; }

        public AudioClip[] randomSoundEffects;
        private ParticleSystem[] particles;
        private AudioSource[] audioSources;
        private AudioSourceSetter[] audioSourceSetters;
        private float destroyTime;

        private void Awake()
        {
            CacheTransform = transform;
            particles = GetComponentsInChildren<ParticleSystem>(true);
            audioSources = GetComponentsInChildren<AudioSource>(true);
            audioSourceSetters = GetComponentsInChildren<AudioSourceSetter>(true);
        }
        
        protected override void PushBack()
        {
            OnPushBack();
            if (ObjectPrefab != null)
                PoolSystem.PushBack(this);
            else if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            if (destroyTime >= 0 && destroyTime - Time.time <= 0)
            {
                PushBack();
            }
        }

        private void LateUpdate()
        {
            if (FollowingTarget != null)
            {
                CacheTransform.position = FollowingTarget.position;
                CacheTransform.rotation = FollowingTarget.rotation;
            }
            else if (intendToFollowingTarget)
            {
                // Don't push back immediately
                DestroyEffect();
            }
        }

        public void DestroyEffect()
        {
            if (particles != null && particles.Length > 0)
            {
                foreach (ParticleSystem particle in particles)
                {
                    if (!particle)
                        continue;
                    ParticleSystem.MainModule mainEmitter = particle.main;
                    mainEmitter.loop = false;
                }
            }
            if (audioSources != null && audioSources.Length > 0)
            {
                foreach (AudioSource audioSource in audioSources)
                {
                    if (!audioSource)
                        continue;
                    audioSource.loop = false;
                }
            }
            destroyTime = Time.time + lifeTime;
        }

        public override void InitPrefab()
        {
            if (this == null)
            {
                Debug.LogWarning("The Game Effect is null, this should not happens " + this);
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
            Play();
            base.OnGetInstance();
        }

        /// <summary>
        /// Play particle effects and an audio
        /// </summary>
        public virtual void Play()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            // Prepare destroy time
            destroyTime = isLoop ? -1 : Time.time + lifeTime;
            // Play random audio
            float volume = AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level;
            if (randomSoundEffects.Length > 0)
            {
                AudioClip soundEffect = randomSoundEffects[Random.Range(0, randomSoundEffects.Length)];
                if (soundEffect != null)
                    AudioSource.PlayClipAtPoint(soundEffect, CacheTransform.position, volume);
            }
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
