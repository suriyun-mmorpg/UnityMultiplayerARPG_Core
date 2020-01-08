using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class CharacterFootstepSoundComponent : BaseGameEntityComponent<BaseGameEntity>
    {
        public AudioSource audioSource;
        public FootstepSettings moveFootstepSettings;
        public FootstepSettings sprintFootstepSettings;
        public FootstepSettings crouchFootstepSettings;
        public FootstepSettings crawlFootstepSettings;
        public FootstepSettings swimFootstepSettings;

        #region Deprecated settings
        [HideInInspector]
        public FootstepSoundData soundData;
        [HideInInspector]
        [Tooltip("This is delay to play future footstep sounds")]
        public float stepDelay = 0.35f;
        [HideInInspector]
        [Tooltip("This is threshold to play footstep sounds, for example if this value is 0.1 and velocity.magnitude more or equals to 0.1 it will play sounds")]
        public float stepThreshold = 0.1f;
        [HideInInspector]
        [Range(0f, 1f)]
        public float randomVolumeMin = 0.75f;
        [HideInInspector]
        [Range(0f, 1f)]
        public float randomVolumeMax = 1f;
        [HideInInspector]
        [Range(-3f, 3f)]
        public float randomPitchMin = 0.75f;
        [HideInInspector]
        [Range(-3f, 3f)]
        public float randomPitchMax = 1f;
        #endregion

        private FootstepSettings currentFootstepSettings;
        private float delayCounter = 0f;

        public override void EntityAwake()
        {
            base.EntityAwake();
            MigrateSettings();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (MigrateSettings())
                EditorUtility.SetDirty(this);
        }
#endif

        private bool MigrateSettings()
        {
            if (soundData.randomAudioClips != null && soundData.randomAudioClips.Length > 0 &&
                (moveFootstepSettings == null || 
                moveFootstepSettings.soundData.randomAudioClips == null || 
                moveFootstepSettings.soundData.randomAudioClips.Length == 0))
            {
                Debug.LogWarning("[CharacterFootstepSoundComponent] Migration run to setup old footstep settings to new footstep settings due to codes structure changes");
                moveFootstepSettings = new FootstepSettings()
                {
                    soundData = soundData,
                    stepDelay = stepDelay,
                    stepThreshold = stepThreshold,
                    randomVolumeMin = randomVolumeMin,
                    randomVolumeMax = randomVolumeMax,
                    randomPitchMin = randomPitchMin,
                    randomPitchMax = randomPitchMax,
                };
                return true;
            }
            return false;
        }

        public override sealed void EntityUpdate()
        {
            // Play sound on clients only
            if (!IsClient || audioSource == null)
            {
                enabled = false;
                return;
            }

            audioSource.mute = !AudioManager.Singleton.sfxVolumeSetting.IsOn;

            if (CacheEntity.MovementState.HasFlag(MovementState.IsUnderWater))
            {
                currentFootstepSettings = swimFootstepSettings;
            }
            else
            {
                switch (CacheEntity.ExtraMovementState)
                {
                    case ExtraMovementState.IsSprinting:
                        currentFootstepSettings = sprintFootstepSettings;
                        break;
                    case ExtraMovementState.IsCrouching:
                        currentFootstepSettings = crouchFootstepSettings;
                        break;
                    case ExtraMovementState.IsCrawling:
                        currentFootstepSettings = crawlFootstepSettings;
                        break;
                    default:
                        currentFootstepSettings = moveFootstepSettings;
                        break;
                }
            }

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
            if (delayCounter >= currentFootstepSettings.stepDelay / CacheEntity.MoveAnimationSpeedMultiplier)
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

            // Don't play sound while muting footstep sound
            if (CacheEntity.MuteFootstepSound)
                return;

            // Don't play sound while passenging vehicle
            if (CacheEntity.PassengingVehicleEntity != null)
                return;

            audioSource.clip = currentFootstepSettings.soundData.GetRandomedAudioClip();
            audioSource.pitch = Random.Range(currentFootstepSettings.randomPitchMin, currentFootstepSettings.randomPitchMax);
            audioSource.volume = Random.Range(currentFootstepSettings.randomVolumeMin, currentFootstepSettings.randomVolumeMax) * (AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
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

    [System.Serializable]
    public class FootstepSettings
    {
        public FootstepSoundData soundData;
        [Tooltip("This is delay to play next footstep sounds")]
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
    }
}
