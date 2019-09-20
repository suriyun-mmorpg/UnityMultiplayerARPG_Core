using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class GameEffect : MonoBehaviour
    {
        public enum DestroyMode
        {
            DestroyGameObject,
            DeactivateGameObject,
        }
        public string effectSocket;
        public bool isLoop;
        public float lifeTime;
        public DestroyMode destroyMode;
        public Transform followingTarget;

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

        public AudioClip[] randomSoundEffects;
        private float volume;
        private ParticleSystem[] particles;
        private AudioSource[] audioSources;
        private float destroyTime;
        private bool isStarted;

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

        private void Update()
        {
            if (followingTarget != null)
            {
                CacheTransform.position = followingTarget.position;
                CacheTransform.rotation = followingTarget.rotation;
            }

            if (destroyTime >= 0 && destroyTime - Time.time <= 0)
            {
                if (destroyMode == DestroyMode.DestroyGameObject)
                    Destroy(gameObject);
                else
                    gameObject.SetActive(false);
            }
        }

        public void DestroyEffect()
        {
            foreach (ParticleSystem particle in particles)
            {
                if (particle == null)
                    continue;
                ParticleSystem.MainModule mainEmitter = particle.main;
                mainEmitter.loop = false;
            }
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource == null)
                    continue;
                audioSource.loop = false;
            }
            destroyTime = Time.time + lifeTime;
        }

        public GameEffect InstantiateTo(Transform parent)
        {
            GameEffect newEffect = Instantiate(this, parent);
            newEffect.transform.localPosition = Vector3.zero;
            newEffect.transform.localEulerAngles = Vector3.zero;
            newEffect.transform.localScale = Vector3.one;
            return newEffect;
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
            if (particles != null)
            {
                foreach (ParticleSystem particle in particles)
                {
                    if (particle == null)
                        continue;
                    particle.Play();
                }
            }
            // Play audio sources
            if (audioSources != null)
            {
                foreach (AudioSource audioSource in audioSources)
                {
                    if (audioSource == null)
                        continue;
                    audioSource.volume = volume;
                    audioSource.Play();
                }
            }
        }
    }
}
