using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseEquipmentEntity : MonoBehaviour, IPoolDescriptorCollection
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

        [Tooltip("These game effects must placed as this children, it will active when launch (can place muzzle effects here)")]
        public GameEffect[] weaponLaunchEffects;
        [Tooltip("These game effects must placed as this children, it will active when launch (can place muzzle sound effects here)")]
        public AudioClip[] weaponLaunchSoundEffects;
        [Tooltip("These game effect prefabs will, it will instantiate to container when launch (can place muzzle effects here)")]
        public GameEffectPoolContainer[] poolingWeaponLaunchEffects;
        [Tooltip("This is overriding missile damage transform, if this is not empty, it will spawn missile damage entity from this transform")]
        public Transform missileDamageTransform;

        public IEnumerable<IPoolDescriptor> PoolDescriptors
        {
            get
            {
                List<IPoolDescriptor> effects = new List<IPoolDescriptor>();
                foreach (GameEffectPoolContainer container in poolingWeaponLaunchEffects)
                {
                    effects.Add(container.prefab);
                }
                return effects;
            }
        }

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
            if (!gameObject.activeInHierarchy)
                return;

            if (weaponLaunchEffects != null && weaponLaunchEffects.Length > 0)
                weaponLaunchEffects[Random.Range(0, weaponLaunchEffects.Length)].Play();

            if (weaponLaunchSoundEffects != null && weaponLaunchSoundEffects.Length > 0)
                AudioSource.PlayClipAtPoint(weaponLaunchSoundEffects[Random.Range(0, weaponLaunchEffects.Length)], transform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);

            if (poolingWeaponLaunchEffects != null && poolingWeaponLaunchEffects.Length > 0)
                poolingWeaponLaunchEffects[Random.Range(0, poolingWeaponLaunchEffects.Length)].GetInstance();
        }

        public abstract void OnLevelChanged(int level);
    }
}
