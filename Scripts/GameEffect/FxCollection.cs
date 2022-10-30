using UnityEngine;

namespace MultiplayerARPG
{
    public class FxCollection
    {
        private ParticleSystem[] particles;
        private LineRenderer[] lineRenderers;
        private AudioSource[] audioSources;
        private AudioSourceSetter[] audioSourceSetters;
        private bool[] particleDefaultLoops;
        private bool[] lineRendererDefaultLoops;
        private bool[] audioSourceDefaultLoops;

        public FxCollection(GameObject gameObject)
        {
            int i;
            particles = gameObject.GetComponentsInChildren<ParticleSystem>(true);
            particleDefaultLoops = new bool[particles.Length];
            for (i = 0; i < particles.Length; ++i)
            {
                particleDefaultLoops[i] = particles[i].main.loop;
            }
            lineRenderers = gameObject.GetComponentsInChildren<LineRenderer>(true);
            lineRendererDefaultLoops = new bool[lineRenderers.Length];
            for (i = 0; i < lineRenderers.Length; ++i)
            {
                lineRendererDefaultLoops[i] = lineRenderers[i].loop;
            }
            audioSources = gameObject.GetComponentsInChildren<AudioSource>(true);
            audioSourceDefaultLoops = new bool[audioSources.Length];
            for (i = 0; i < audioSources.Length; ++i)
            {
                audioSourceDefaultLoops[i] = audioSources[i].loop;
            }
            audioSourceSetters = gameObject.GetComponentsInChildren<AudioSourceSetter>(true);
        }

        public void RevertLoop()
        {
            int i;
            ParticleSystem.MainModule mainEmitter;
            for (i = 0; i < particles.Length; ++i)
            {
                mainEmitter = particles[i].main;
                mainEmitter.loop = particleDefaultLoops[i];
            }
            for (i = 0; i < lineRenderers.Length; ++i)
            {
                lineRenderers[i].loop = lineRendererDefaultLoops[i];
            }
            for (i = 0; i < audioSources.Length; ++i)
            {
                audioSources[i].loop = audioSourceDefaultLoops[i];
            }
        }

        public void SetLoop(bool loop)
        {
            int i;
            ParticleSystem.MainModule mainEmitter;
            for (i = 0; i < particles.Length; ++i)
            {
                mainEmitter = particles[i].main;
                mainEmitter.loop = loop;
            }
            for (i = 0; i < lineRenderers.Length; ++i)
            {
                lineRenderers[i].loop = loop;
            }
            for (i = 0; i < audioSources.Length; ++i)
            {
                audioSources[i].loop = loop;
            }
        }

        public void InitPrefab()
        {
            // Prepare particles
            ParticleSystem.MainModule mainEmitter;
            foreach (ParticleSystem particle in particles)
            {
                mainEmitter = particle.main;
                mainEmitter.playOnAwake = false;
            }
            // Prepare audio sources
            foreach (AudioSource audioSource in audioSources)
            {
                audioSource.playOnAwake = false;
            }
            // Prepare audio source setters
            foreach (AudioSourceSetter audioSourceSetter in audioSourceSetters)
            {
                audioSourceSetter.playOnAwake = false;
                audioSourceSetter.playOnEnable = false;
            }
        }

        public void Play()
        {
            if (Application.isBatchMode)
                return;
            int i;
            // Play particles
            ParticleSystem.MainModule mainEmitter;
            for (i = 0; i < particles.Length; ++i)
            {
                mainEmitter = particles[i].main;
                mainEmitter.loop = particleDefaultLoops[i];
                particles[i].Play();
            }
            // Revert line renderers loop
            for (i = 0; i < lineRenderers.Length; ++i)
            {
                lineRenderers[i].loop = lineRendererDefaultLoops[i];
            }
            // Play audio sources
            float volume = AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level;
            for (i = 0; i < audioSources.Length; ++i)
            {
                audioSources[i].loop = audioSourceDefaultLoops[i];
                audioSources[i].volume = volume;
                audioSources[i].Play();
            }
            // Play audio source setters
            for (i = 0; i < audioSourceSetters.Length; ++i)
            {
                audioSourceSetters[i].Play();
            }
        }

        public void Stop()
        {
            if (Application.isBatchMode)
                return;
            int i;
            // Stop particles
            for (i = 0; i < particles.Length; ++i)
            {
                particles[i].Stop();
            }
            // Stop audio sources
            for (i = 0; i < audioSources.Length; ++i)
            {
                audioSources[i].Stop();
            }
            // Stop audio source setters
            for (i = 0; i < audioSourceSetters.Length; ++i)
            {
                audioSourceSetters[i].Stop();
            }
        }
    }
}
