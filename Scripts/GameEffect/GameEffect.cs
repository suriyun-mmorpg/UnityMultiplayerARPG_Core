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
        
        public Transform CacheTransform { get; private set; }

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

        public AudioClip[] randomSoundEffects;
        private float volume;
        private ParticleSystem[] particles;
        private AudioSource[] audioSources;
        private float destroyTime;
        private bool isStarted;

        private void Awake()
        {
            CacheTransform = transform;
        }

        private void Start()
        {
            if (!isStarted)
            {
                Play();
                isStarted = true;
            }
        }

        private void OnEnable()
        {
            if (isStarted)
                Play();
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

        public GameEffect GetInstance()
        {
            // `this` is prefab
            return PoolSystem.GetInstance(this);
        }

        public void Play()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                return;
            }
            // Prepare particles
            if (particles == null)
                particles = GetComponentsInChildren<ParticleSystem>();
            // Prepare audio sources
            if (audioSources == null)
                audioSources = GetComponentsInChildren<AudioSource>();
            // Prepare destroy time
            destroyTime = isLoop ? -1 : Time.time + lifeTime;
            // Play random audio
            volume = AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level;
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
