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

        [SerializeField]
        private EquipmentModel[] sheathModels = new EquipmentModel[0];
        public EquipmentModel[] SheathModels
        {
            get { return sheathModels; }
            set { sheathModels = value; }
        }

        [SerializeField]
        private EquipmentModel[] offHandSheathModels = new EquipmentModel[0];
        public EquipmentModel[] OffHandSheathModels
        {
            get { return offHandSheathModels; }
            set { offHandSheathModels = value; }
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
        private DamageIncremental damageAmount = default;
        public DamageIncremental DamageAmount
        {
            get { return damageAmount; }
        }

        [SerializeField]
        private IncrementalMinMaxFloat harvestDamageAmount = default;
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
        private MovementRestriction movementRestrictionWhileReloading = default;
        public MovementRestriction MovementRestrictionWhileReloading
        {
            get { return movementRestrictionWhileReloading; }
        }

        [SerializeField]
        private MovementRestriction movementRestrictionWhileCharging = default;
        public MovementRestriction MovementRestrictionWhileCharging
        {
            get { return movementRestrictionWhileCharging; }
        }

        [SerializeField]
        private MovementRestriction movementRestrictionWhileAttacking = default;
        public MovementRestriction MovementRestrictionWhileAttacking
        {
            get { return movementRestrictionWhileAttacking; }
        }

        [SerializeField]
        private ActionRestriction attackRestriction = default;
        public ActionRestriction AttackRestriction
        {
            get { return attackRestriction; }
        }

        [SerializeField]
        private ActionRestriction reloadRestriction = default;
        public ActionRestriction ReloadRestriction
        {
            get { return reloadRestriction; }
        }

        [SerializeField]
        private int ammoCapacity = 0;
        public int AmmoCapacity
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
        private CrosshairSetting crosshairSetting = default;
        public CrosshairSetting CrosshairSetting
        {
            get { return crosshairSetting; }
        }

        [HideInInspector]
        [SerializeField]
        private AudioClip launchClip = null;

        [HideInInspector]
        [SerializeField]
        private AudioClip[] launchClips = new AudioClip[0];

        [SerializeField]
        private AudioClipWithVolumeSettings[] launchClipSettings = new AudioClipWithVolumeSettings[0];

        public AudioClipWithVolumeSettings LaunchClip
        {
            get
            {
                if (launchClipSettings != null && launchClipSettings.Length > 0)
                    return launchClipSettings[Random.Range(0, launchClipSettings.Length - 1)];
                return null;
            }
        }

        [HideInInspector]
        [SerializeField]
        private AudioClip reloadClip = null;

        [HideInInspector]
        [SerializeField]
        private AudioClip[] reloadClips = new AudioClip[0];

        [SerializeField]
        private AudioClipWithVolumeSettings[] reloadClipSettings = new AudioClipWithVolumeSettings[0];

        public AudioClipWithVolumeSettings ReloadClip
        {
            get
            {
                if (reloadClipSettings != null && reloadClipSettings.Length > 0)
                    return reloadClipSettings[Random.Range(0, reloadClipSettings.Length - 1)];
                return null;
            }
        }

        [HideInInspector]
        [SerializeField]
        private AudioClip reloadedClip = null;

        [HideInInspector]
        [SerializeField]
        private AudioClip[] reloadedClips = new AudioClip[0];

        [SerializeField]
        private AudioClipWithVolumeSettings[] reloadedClipSettings = new AudioClipWithVolumeSettings[0];

        public AudioClipWithVolumeSettings ReloadedClip
        {
            get
            {
                if (reloadedClipSettings != null && reloadedClipSettings.Length > 0)
                    return reloadedClipSettings[Random.Range(0, reloadedClipSettings.Length - 1)];
                return null;
            }
        }

        [HideInInspector]
        [SerializeField]
        private AudioClip emptyClip = null;

        [HideInInspector]
        [SerializeField]
        private AudioClip[] emptyClips = new AudioClip[0];

        [SerializeField]
        private AudioClipWithVolumeSettings[] emptyClipSettings = new AudioClipWithVolumeSettings[0];

        public AudioClipWithVolumeSettings EmptyClip
        {
            get
            {
                if (emptyClipSettings != null && emptyClipSettings.Length > 0)
                    return emptyClipSettings[Random.Range(0, emptyClipSettings.Length - 1)];
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
        private float chargeDuration = 0;
        public float ChargeDuration
        {
            get { return chargeDuration; }
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
            if (MigrateAudioClips(ref launchClip, ref launchClips, ref launchClipSettings))
                hasChanges = true;
            if (MigrateAudioClips(ref reloadClip, ref reloadClips, ref reloadClipSettings))
                hasChanges = true;
            if (MigrateAudioClips(ref reloadedClip, ref reloadedClips, ref reloadedClipSettings))
                hasChanges = true;
            if (MigrateAudioClips(ref emptyClip, ref emptyClips, ref emptyClipSettings))
                hasChanges = true;
            return hasChanges || base.Validate();
        }

        private bool MigrateAudioClips(ref AudioClip singleClip, ref AudioClip[] multipleClips, ref AudioClipWithVolumeSettings[] destinationSettings)
        {
            if (singleClip == null && (multipleClips == null || multipleClips.Length == 0))
                return false;

            bool hasChanges = false;

            List<AudioClip> clips = new List<AudioClip>();
            if (multipleClips != null && multipleClips.Length > 0)
            {
                clips.AddRange(multipleClips);
                multipleClips = null;
                hasChanges = true;
            }
            if (singleClip != null && !clips.Contains(singleClip))
            {
                clips.Add(singleClip);
                singleClip = null;
                hasChanges = true;
            }
            if (!hasChanges)
                return false;

            List<AudioClipWithVolumeSettings> clipSettings = new List<AudioClipWithVolumeSettings>();
            if (destinationSettings != null && destinationSettings.Length > 0)
                clipSettings.AddRange(destinationSettings);
            for (int i = 0; i < clips.Count; ++i)
            {
                clipSettings.Add(new AudioClipWithVolumeSettings()
                {
                    audioClip = clips[i],
                    minRandomVolume = 1f,
                    maxRandomVolume = 1f,
                });
            }
            destinationSettings = clipSettings.ToArray();
            return true;
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
