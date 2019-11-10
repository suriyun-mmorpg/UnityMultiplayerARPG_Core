using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterFootstepSoundComponent : BaseGameEntityComponent<BaseGameEntity>
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
            // Play sound on clients only
            if (!IsClient || audioSource == null)
            {
                enabled = false;
                return;
            }

            audioSource.mute = !AudioManager.Singleton.sfxVolumeSetting.IsOn;
            if (CacheEntity is BaseCharacterEntity)
                audioSource.mute = audioSource.mute || (CacheEntity as BaseCharacterEntity).GetCaches().MuteFootstepSound;

            if (!CacheEntity.MovementState.HasFlag(MovementState.Forward) &&
                !CacheEntity.MovementState.HasFlag(MovementState.Backward) &&
                !CacheEntity.MovementState.HasFlag(MovementState.Right) &&
                !CacheEntity.MovementState.HasFlag(MovementState.Left))
            {
                // No movement
                delayCounter = 0f;
                return;
            }

            delayCounter += Time.deltaTime;
            if (delayCounter >= stepDelay / CacheEntity.MoveAnimationSpeedMultiplier)
            {
                if (CacheEntity.MovementState.HasFlag(MovementState.IsGrounded))
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
            audioSource.volume = Random.Range(randomVolumeMin, randomVolumeMax) * (AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
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
