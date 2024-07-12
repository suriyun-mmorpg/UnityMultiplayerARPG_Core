using Insthync.AddressableAssetTools;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AudioClipWithVolumeSettings
    {
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [AddressableAssetConversion(nameof(addressableAudioClip))]
        public AudioClip audioClip;
#endif
        public AudioClip AudioClip
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return audioClip;
#else
                return null;
#endif
            }
        }

        public AssetReferenceAudioClip addressableAudioClip;
        public AssetReferenceAudioClip AddressableAudioClip
        {
            get { return addressableAudioClip; }
        }

        [Range(0f, 1f)]
        public float minRandomVolume = 1f;
        [Range(0f, 1f)]
        public float maxRandomVolume = 1f;

        public float GetRandomedVolume()
        {
            return Random.Range(minRandomVolume, maxRandomVolume);
        }

        public async void Play(AudioSource source)
        {
#if !UNITY_SERVER
            AudioManager.PlaySfxClipAtAudioSource(await AddressableAudioClip.GetOrLoadObjectAsyncOrUseAsset(AudioClip), source, GetRandomedVolume());
#endif
        }
    }
}
