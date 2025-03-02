using Insthync.AddressableAssetTools;
using Insthync.AudioManager;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AudioClipWithVolumeSettings : IAddressableAssetConversable
    {
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
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

        public void ProceedAddressableAssetConversion(string groupName)
        {
#if UNITY_EDITOR
            AddressableEditorUtils.ConvertObjectRefToAddressable(ref audioClip, ref addressableAudioClip, groupName);
#endif
        }
    }
}
