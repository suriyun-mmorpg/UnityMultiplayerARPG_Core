using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterFootstepSoundComponent : BaseCharacterComponent
    {
        public AudioSource audioSource;
        public FootstepSoundData soundData;
        [Tooltip("This is delay to play future footstep sounds")]
        public float stepDelay = 0.35f;
        [Tooltip("This is threshold to play footstep sounds, for example if this value is 0.1 and velocity.magnitude more or equals to 0.1 it will play sounds")]
        public float stepThreshold = 0.1f;
        [Range(0f, 1f)]
        public float randomVolumeMin = 0.75f;
        [Range(0f, 1f)]
        public float randomVolumeMax = 1f;
        [Range(-3f, 3f)]
        public float randomPitchMin = 0.75f;
        [Range(-3f, 3f)]
        public float randomPitchMax = 1f;

        private float delayCounter = 0f;

        protected void Update()
        {
            if (audioSource == null)
                return;

            audioSource.mute = !AudioManager.Singleton.sfxVolumeSetting.IsOn;

            if (!CacheCharacterEntity.MovementState.HasFlag(MovementFlag.Forward) &&
                !CacheCharacterEntity.MovementState.HasFlag(MovementFlag.Backward) &&
                !CacheCharacterEntity.MovementState.HasFlag(MovementFlag.Right) &&
                !CacheCharacterEntity.MovementState.HasFlag(MovementFlag.Left))
            {
                delayCounter = 0f;
                return;
            }

            delayCounter += Time.deltaTime;
            if (delayCounter >= stepDelay / CacheCharacterEntity.MoveAnimationSpeedMultiplier)
            {
                if (CacheCharacterEntity.MovementState.HasFlag(MovementFlag.IsGrounded))
                    PlaySound();

                delayCounter = 0f;
            }
        }

        public void PlaySound()
        {
            if (audioSource == null)
                return;

            audioSource.clip = soundData.GetRandomedAudioClip();
            audioSource.pitch = Random.Range(randomPitchMin, randomPitchMax);
            audioSource.volume = Random.Range(randomVolumeMin, randomVolumeMax);
            audioSource.Play();
        }
    }

    [System.Serializable]
    public struct FootstepSoundData
    {
        public AudioClip[] randomAudioClips;

        public AudioClip GetRandomedAudioClip()
        {
            if (randomAudioClips == null || randomAudioClips.Length == 0)
                return null;
            return randomAudioClips[Random.Range(0, randomAudioClips.Length)];
        }
    }
}
