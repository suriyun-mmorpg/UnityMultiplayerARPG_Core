using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public abstract partial class BaseEquipmentEntity : MonoBehaviour, IPoolDescriptorCollection
    {
        public BaseCharacterModel CharacterModel { get; set; }
        public string EquipPosition { get; set; }

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

        [Tooltip("These game effects must placed as this children, it will be activated when launch (can place muzzle effects here)")]
        public GameEffect[] weaponLaunchEffects;
        [Tooltip("These game effects prefabs will instantiates to container when launch (can place muzzle effects here)")]
        public GameEffectPoolContainer[] poolingWeaponLaunchEffects;
        [Tooltip("This is overriding missile damage transform, if this is not empty, it will spawn missile damage entity from this transform")]
        public Transform missileDamageTransform;
        public UnityEvent onSetup = new UnityEvent();
        public UnityEvent onEnable = new UnityEvent();
        public UnityEvent onDisable = new UnityEvent();
        public UnityEvent onPlayLaunch = new UnityEvent();
        public UnityEvent onPlayReload = new UnityEvent();
        public UnityEvent onPlayReloaded = new UnityEvent();
        public UnityEvent onPlayCharge = new UnityEvent();

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

        public virtual void Setup(BaseCharacterModel characterModel, string equipPosition, int level)
        {
            CharacterModel = characterModel;
            EquipPosition = equipPosition;
            Level = level;
            onSetup.Invoke();
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

            onEnable.Invoke();
        }

        protected virtual void OnDisable()
        {
            onDisable.Invoke();
        }

        public virtual void PlayLaunch()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (weaponLaunchEffects != null && weaponLaunchEffects.Length > 0)
                weaponLaunchEffects[Random.Range(0, weaponLaunchEffects.Length)].Play();

            if (poolingWeaponLaunchEffects != null && poolingWeaponLaunchEffects.Length > 0)
                poolingWeaponLaunchEffects[Random.Range(0, poolingWeaponLaunchEffects.Length)].GetInstance();

            onPlayLaunch.Invoke();
        }

        public virtual void PlayReload()
        {
            onPlayReload.Invoke();
        }

        public virtual void PlayReloaded()
        {
            onPlayReloaded.Invoke();
        }

        public virtual void PlayCharge()
        {
            onPlayCharge.Invoke();
        }

        [ContextMenu("Set `missileDamageTransform` as `poolingWeaponLaunchEffects` container")]
        public void SetMissileDamageTransformAsPoolingEffectsContainer()
        {
            if (poolingWeaponLaunchEffects != null && poolingWeaponLaunchEffects.Length > 0)
            {
                for (int i = 0; i < poolingWeaponLaunchEffects.Length; ++i)
                {
                    GameEffectPoolContainer container = poolingWeaponLaunchEffects[i];
                    container.container = missileDamageTransform;
                    poolingWeaponLaunchEffects[i] = container;
                }
            }
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
