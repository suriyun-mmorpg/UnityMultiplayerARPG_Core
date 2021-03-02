using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterDeathSoundComponent : BaseGameEntityComponent<BaseCharacterEntity>
    {
        public AudioSource audioSource;
        public DeathSoundData soundData;
        [Range(0f, 1f)]
        public float volume = 1f;
        private bool dirtyIsDead;

        public override void EntityUpdate()
        {
            if (dirtyIsDead != Entity.IsDead())
            {
                dirtyIsDead = Entity.IsDead();
                if (dirtyIsDead)
                    PlaySound();
            }
        }

        public void PlaySound()
        {
            if (audioSource == null)
                return;

            audioSource.clip = soundData.GetRandomedAudioClip();
            audioSource.volume = volume;
            audioSource.Play();
        }
    }

    [System.Serializable]
    public struct DeathSoundData
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
