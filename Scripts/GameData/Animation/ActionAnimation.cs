using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ActionAnimationOverrideData
    {
        public static readonly ActionAnimationOverrideData Empty = new ActionAnimationOverrideData();
        public CharacterModel target;
        [Tooltip("Must set it to override default animation data")]
        public AnimationClip clip;
        [Tooltip("Set it more than zero to override default trigger duration rate")]
        [Range(0f, 1f)]
        public float triggerDurationRate;
        public float extraDuration;
        [Tooltip("Set it length more than zero to override default audio clips")]
        public AudioClip[] audioClips;
        public bool IsEmpty()
        {
            return Equals(Empty);
        }
    }

    [System.Serializable]
    public class ActionAnimation
    {
        private static uint idCount = 0;
        protected uint? id;
        public uint Id
        {
            get { return !id.HasValue ? 0: id.Value; }
        }

        [SerializeField]
        private AnimationClip clip;
        [Range(0.01f, 1f)]
        [SerializeField]
        private float triggerDurationRate;
        [Tooltip("Extra duration after played animation clip")]
        [SerializeField]
        private float extraDuration;
        [Tooltip("Audio clips playing randomly while play this animation (not loop)")]
        [SerializeField]
        private AudioClip[] audioClips;
        [Tooltip("Override clip for target model")]
        [SerializeField]
        private ActionAnimationOverrideData[] overrideData;

        private Dictionary<int, ActionAnimationOverrideData> cacheOverrideData;
        public Dictionary<int, ActionAnimationOverrideData> CacheOverrideData
        {
            get
            {
                if (cacheOverrideData == null)
                {
                    cacheOverrideData = new Dictionary<int, ActionAnimationOverrideData>();
                    if (overrideData != null)
                    {
                        foreach (var overrideDataEntry in overrideData)
                        {
                            if (overrideDataEntry.target == null || overrideDataEntry.clip == null)
                                continue;
                            cacheOverrideData[overrideDataEntry.target.DataId] = overrideDataEntry;
                        }
                    }
                }
                return cacheOverrideData;
            }
        }

        /// <summary>
        /// Initialize action id, will return false if it's already initialized
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Initialize()
        {
            if (id.HasValue)
                return false;

            ++idCount;
            id = idCount;
            return true;
        }

        public static void ResetId()
        {
            idCount = 0;
        }

        private AudioClip GetRandomAudioClip(AudioClip[] audioClips)
        {
            AudioClip clip = null;
            if (audioClips != null && audioClips.Length > 0)
                clip = audioClips[Random.Range(0, audioClips.Length)];
            return clip;
        }

        public bool GetData(CharacterModel model, out AnimationClip clip, out float triggerDuration, out float extraDuration, out AudioClip audioClip)
        {
            clip = this.clip;
            extraDuration = this.extraDuration;
            var triggerDurationRate = this.triggerDurationRate;
            var audioClips = this.audioClips;
            ActionAnimationOverrideData overrideData;
            if (CacheOverrideData.TryGetValue(model.DataId, out overrideData))
            {
                clip = overrideData.clip;
                if (overrideData.triggerDurationRate > 0)
                    triggerDurationRate = overrideData.triggerDurationRate;
                extraDuration = overrideData.extraDuration;
                if (overrideData.audioClips != null && overrideData.audioClips.Length > 0)
                    audioClips = overrideData.audioClips;
            }
            triggerDuration = (clip != null ? clip.length : 0) * triggerDurationRate;
            audioClip = GetRandomAudioClip(audioClips);
            return clip != null;
        }
    }
}
