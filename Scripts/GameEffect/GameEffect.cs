using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class GameEffect : MonoBehaviour, IPoolDescriptor
    {
        public string effectSocket;
        public bool isLoop;
        public float lifeTime;
        public Transform followingTarget;

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

        protected virtual void PushBack(float delay)
        {
            Invoke("PushBack", delay);
        }

        protected virtual void PushBack()
        {
            PoolSystem.PushBack(this);
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
            if (followingTarget != null)
            {
                CacheTransform.position = followingTarget.position;
                CacheTransform.rotation = followingTarget.rotation;
            }
            else
            {
                PushBack();
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

        public void InitPrefab()
        {
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
        }

        public virtual void OnGetInstance()
        {
            Play();
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
