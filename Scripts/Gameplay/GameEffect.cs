using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEffect : MonoBehaviour
{
    public bool isLoop;
    public float lifeTime;

    private Transform tempTransform;
    public Transform TempTransform
    {
        get
        {
            if (tempTransform == null)
                tempTransform = GetComponent<Transform>();
            return tempTransform;
        }
    }

    private ParticleSystem[] particles;
    private AudioSource[] audioSources;

    private void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var particle in particles)
        {
            particle.Play();
        }
        audioSources = GetComponentsInChildren<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            audioSource.Play();
        }
    }

    private void Start()
    {
        if (!isLoop)
            Destroy(gameObject, lifeTime);
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

    public GameEffect InstantiateTo(Transform parent, bool asChildren = true)
    {
        var effect = Instantiate(this, asChildren ? parent : null);
        effect.TempTransform.localPosition = Vector3.zero;
        effect.TempTransform.localEulerAngles = Vector3.zero;
        effect.TempTransform.localScale = Vector3.one;
        effect.gameObject.SetActive(true);
        return effect;
    }
}
