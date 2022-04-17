using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.WEAPON_ITEM_FILE, menuName = GameDataMenuConsts.WEAPON_ITEM_MENU, order = GameDataMenuConsts.WEAPON_ITEM_ORDER)]
    public partial class WeaponItem : BaseEquipmentItem, IWeaponItem
    {
        public override string TypeTitle
        {
            get { return WeaponType.Title; }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Weapon; }
        }

        [Category("In-Scene Objects/Appearance")]
        [SerializeField]
        private EquipmentModel[] offHandEquipmentModels = new EquipmentModel[0];
        public EquipmentModel[] OffHandEquipmentModels
        {
            get { return offHandEquipmentModels; }
            set { offHandEquipmentModels = value; }
        }

        [Category("Equipment Settings")]
        [Header("Weapon Settings")]
        [SerializeField]
        private WeaponType weaponType = null;
        public WeaponType WeaponType
        {
            get { return weaponType; }
        }

        [SerializeField]
        private DamageIncremental damageAmount = default(DamageIncremental);
        public DamageIncremental DamageAmount
        {
            get { return damageAmount; }
        }

        [SerializeField]
        private IncrementalMinMaxFloat harvestDamageAmount = default(IncrementalMinMaxFloat);
        public IncrementalMinMaxFloat HarvestDamageAmount
        {
            get { return harvestDamageAmount; }
        }

        [SerializeField]
        private float moveSpeedRateWhileReloading = 1f;
        public float MoveSpeedRateWhileReloading
        {
            get { return moveSpeedRateWhileReloading; }
        }

        [SerializeField]
        private float moveSpeedRateWhileCharging = 1f;
        public float MoveSpeedRateWhileCharging
        {
            get { return moveSpeedRateWhileCharging; }
        }

        [SerializeField]
        private float moveSpeedRateWhileAttacking = 0f;
        public float MoveSpeedRateWhileAttacking
        {
            get { return moveSpeedRateWhileAttacking; }
        }

        [SerializeField]
        private short ammoCapacity = 0;
        public short AmmoCapacity
        {
            get { return ammoCapacity; }
        }

        [SerializeField]
        private BaseWeaponAbility weaponAbility = null;
        public BaseWeaponAbility WeaponAbility
        {
            get { return weaponAbility; }
        }

        [SerializeField]
        private CrosshairSetting crosshairSetting = default(CrosshairSetting);
        public CrosshairSetting CrosshairSetting
        {
            get { return crosshairSetting; }
        }

        [HideInInspector]
        [SerializeField]
        private AudioClip launchClip = null;
        [SerializeField]
        private AudioClip[] launchClips = new AudioClip[0];
        public AudioClip LaunchClip
        {
            get
            {
                if (launchClips != null && launchClips.Length > 0)
                    return launchClips[Random.Range(0, launchClips.Length - 1)];
                return null;
            }
        }

        [HideInInspector]
        [SerializeField]
        private AudioClip reloadClip = null;
        [SerializeField]
        private AudioClip[] reloadClips = new AudioClip[0];
        public AudioClip ReloadClip
        {
            get
            {
                if (reloadClips != null && reloadClips.Length > 0)
                    return reloadClips[Random.Range(0, reloadClips.Length - 1)];
                return null;
            }
        }

        [HideInInspector]
        [SerializeField]
        private AudioClip emptyClip = null;
        [SerializeField]
        private AudioClip[] emptyClips = new AudioClip[0];
        public AudioClip EmptyClip
        {
            get
            {
                if (emptyClips != null && emptyClips.Length > 0)
                    return emptyClips[Random.Range(0, emptyClips.Length - 1)];
                return null;
            }
        }

        [SerializeField]
        private FireType fireType = FireType.SingleFire;
        public FireType FireType
        {
            get { return fireType; }
        }

        [SerializeField]
        private Vector2 fireStagger = Vector2.zero;
        public Vector2 FireStagger
        {
            get { return fireStagger; }
        }

        [SerializeField]
        private byte fireSpread = 0;
        public byte FireSpread
        {
            get { return fireSpread; }
        }

        [SerializeField]
        private bool destroyImmediatelyAfterFired = false;
        public bool DestroyImmediatelyAfterFired
        {
            get { return destroyImmediatelyAfterFired; }
        }

        public override bool Validate()
        {
            bool hasChanges = false;
            if (launchClip != null)
            {
                List<AudioClip> clips = new List<AudioClip>(launchClips);
                if (!clips.Contains(launchClip))
                {
                    clips.Add(launchClip);
                    launchClips = clips.ToArray();
                }
                launchClip = null;
                hasChanges = true;
            }
            if (reloadClip != null)
            {
                List<AudioClip> clips = new List<AudioClip>(reloadClips);
                if (!clips.Contains(reloadClip))
                {
                    clips.Add(reloadClip);
                    reloadClips = clips.ToArray();
                }
                reloadClip = null;
                hasChanges = true;
            }
            if (emptyClip != null)
            {
                List<AudioClip> clips = new List<AudioClip>(emptyClips);
                if (!clips.Contains(emptyClip))
                {
                    clips.Add(emptyClip);
                    emptyClips = clips.ToArray();
                }
                emptyClip = null;
                hasChanges = true;
            }
            return hasChanges || base.Validate();
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddDamageElements(DamageAmount);
            GameInstance.AddPoolingWeaponLaunchEffects(OffHandEquipmentModels);
            GameInstance.AddWeaponTypes(WeaponType);
            // Data migration
            GameInstance.MigrateEquipmentEntities(OffHandEquipmentModels);
        }
    }
}