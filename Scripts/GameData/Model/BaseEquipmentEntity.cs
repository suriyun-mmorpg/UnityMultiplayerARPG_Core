using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseEquipmentEntity : MonoBehaviour
    {
        private int level;
        public int Level
        {
            get { return level; }
            set
            {
                if (level != value)
                {
                    level = value;
                    OnLevelChanged(level);
                }
            }
        }

        public GameEffect[] weaponLaunchEffects;
        public AudioClip[] weaponLaunchSoundEffects;
        [Tooltip("This is overriding missile damage transform, if this is not empty, it will spawn missile damage entity from this transform")]
        public Transform missileDamageTransform;
        
        protected virtual void OnEnable()
        {
            if (weaponLaunchEffects != null && weaponLaunchEffects.Length > 0)
            {
                foreach (GameEffect weaponLaunchEffect in weaponLaunchEffects)
                {
                    weaponLaunchEffect.gameObject.SetActive(false);
                }
            }
        }

        public void PlayWeaponLaunchEffect()
        {
            if (weaponLaunchEffects != null && weaponLaunchEffects.Length > 0)
                weaponLaunchEffects[Random.Range(0, weaponLaunchEffects.Length)].Play();

            if (weaponLaunchSoundEffects != null && weaponLaunchSoundEffects.Length > 0)
                AudioSource.PlayClipAtPoint(weaponLaunchSoundEffects[Random.Range(0, weaponLaunchEffects.Length)], transform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);

        }
        public abstract void OnLevelChanged(int level);
    }
}
