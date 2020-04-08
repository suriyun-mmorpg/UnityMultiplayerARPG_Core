using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ProjectileEffect : MonoBehaviour, IPoolDescriptor
    {
        public float speed;
        public float lifeTime = 1;
        [SerializeField]
        private int poolSize = 30;
        public int PoolSize { get { return poolSize; } set { poolSize = value; } }
        public IPoolDescriptor ObjectPrefab { get; set; }
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
            Invoke("PushBack", delay);
        }

        protected virtual void PushBack()
        {
            PoolSystem.PushBack(this);
        }

        protected virtual void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        public virtual void Setup(float distance, float speed)
        {
            this.speed = speed;
            lifeTime = distance / speed;
            PushBack(lifeTime);
        }

        public virtual void InitPrefab()
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
