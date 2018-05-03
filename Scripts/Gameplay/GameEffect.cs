using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEffect : MonoBehaviour
{
    public string effectSocket;
    public bool isLoop;
    public float lifeTime;
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
    private ParticleSystem[] particles;
    private AudioSource[] audioSources;

    private void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
        audioSources = GetComponentsInChildren<AudioSource>();
    }

    private void Start()
    {
        if (randomSoundEffects.Length > 0)
        {
            var soundEffect = randomSoundEffects[Random.Range(0, randomSoundEffects.Length)];
            if (soundEffect != null)
            AudioSource.PlayClipAtPoint(soundEffect, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
        }
        foreach (var particle in particles)
        {
            particle.Play();
        }
        foreach (var audioSource in audioSources)
        {
            audioSource.Play();
        }
        if (!isLoop)
            Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (followingTarget != null)
        {
            CacheTransform.position = followingTarget.position;
            CacheTransform.rotation = followingTarget.rotation;
        }
    }

    public void DestroyEffect()
    {
        foreach (var particle in particles)
        {
            var mainEmitter = particle.main;
            mainEmitter.loop = false;
        }
        foreach (var audioSource in audioSources)
        {
            audioSource.loop = false;
        }
        Destroy(gameObject, lifeTime);
    }

    public GameEffect InstantiateTo(Transform parent)
    {
        var newEffect = Instantiate(this, parent);
        newEffect.transform.localPosition = Vector3.zero;
        newEffect.transform.localEulerAngles = Vector3.zero;
        newEffect.transform.localScale = Vector3.one;
        return newEffect;
    }
}
