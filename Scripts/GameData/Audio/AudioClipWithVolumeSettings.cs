using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AudioClipWithVolumeSettings
    {
        public AudioClip audioClip;
        [Range(0f, 1f)]
        public float minRandomVolume = 1f;
        [Range(0f, 1f)]
        public float maxRandomVolume = 1f;

        public float GetRandomedVolume()
        {
            return Random.Range(minRandomVolume, maxRandomVolume);
        }
    }
}
