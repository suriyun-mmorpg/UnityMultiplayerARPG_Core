using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEffect : MonoBehaviour
{
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

    public AudioClip[] soundEffects;
    private ParticleSystem[] particles;
    private AudioSource[] audioSources;

    private void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
        audioSources = GetComponentsInChildren<AudioSource>();
    }

    private void Start()
    {
        foreach (var soundEffect in soundEffects)
        {
            AudioSource.PlayClipAtPoint(soundEffect, transform.position);
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
}
