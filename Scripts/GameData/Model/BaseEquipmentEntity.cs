using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        [Tooltip("These game effects prefabs will instantiates to container when launch (can place muzzle effects here)")]
        public GameEffectPoolContainer[] poolingWeaponLaunchEffects;
        [Tooltip("This is overriding missile damage transform, if this is not empty, it will spawn missile damage entity from this transform")]
        public Transform missileDamageTransform;

        public IEnumerable<IPoolDescriptor> PoolDescriptors
        {
            get
            {
                List<IPoolDescriptor> effects = new List<IPoolDescriptor>();
                if (poolingWeaponLaunchEffects != null && poolingWeaponLaunchEffects.Length > 0)
                {
                    foreach (GameEffectPoolContainer container in poolingWeaponLaunchEffects)
                    {
                        effects.Add(container.prefab);
                    }
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

            if (poolingWeaponLaunchEffects != null && poolingWeaponLaunchEffects.Length > 0)
                poolingWeaponLaunchEffects[Random.Range(0, poolingWeaponLaunchEffects.Length)].GetInstance();
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            Gizmos.DrawSphere(transform.position, 0.03f);
            Handles.Label(transform.position, name + "(Pivot)");
            if (missileDamageTransform != null)
            {
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(missileDamageTransform.position, 0.03f);
                Handles.Label(missileDamageTransform.position, name + "(MissleDamage)");
            }
        }
#endif

        public abstract void OnLevelChanged(int level);
    }
}
