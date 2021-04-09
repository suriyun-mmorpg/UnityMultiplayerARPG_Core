using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Weapon Item", menuName = "Create GameData/Item/Weapon Item", order = -4889)]
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

        [Header("Weapon Configs")]
        [SerializeField]
        private WeaponType weaponType;
        public WeaponType WeaponType
        {
            get { return weaponType; }
        }

        [SerializeField]
        private EquipmentModel[] offHandEquipmentModels;
        public EquipmentModel[] OffHandEquipmentModels
        {
            get { return offHandEquipmentModels; }
            set { offHandEquipmentModels = value; }
        }

        [SerializeField]
        private DamageIncremental damageAmount;
        public DamageIncremental DamageAmount
        {
            get { return damageAmount; }
        }

        [SerializeField]
        private IncrementalMinMaxFloat harvestDamageAmount;
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
        private short ammoCapacity;
        public short AmmoCapacity
        {
            get { return ammoCapacity; }
        }

        [SerializeField]
        private BaseWeaponAbility weaponAbility;
        public BaseWeaponAbility WeaponAbility
        {
            get { return weaponAbility; }
        }

        [SerializeField]
        private CrosshairSetting crosshairSetting;
        public CrosshairSetting CrosshairSetting
        {
            get { return crosshairSetting; }
        }

        [SerializeField]
        private AudioClip launchClip;
        [SerializeField]
        private AudioClip[] launchClips;
        public AudioClip LaunchClip
        {
            get
            {
                if (launchClips != null && launchClips.Length > 0)
                    return launchClips[Random.Range(0, launchClips.Length - 1)];
                return launchClip;
            }
        }

        [SerializeField]
        private AudioClip reloadClip;
        [SerializeField]
        private AudioClip[] reloadClips;
        public AudioClip ReloadClip
        {
            get
            {
                if (reloadClips != null && reloadClips.Length > 0)
                    return reloadClips[Random.Range(0, reloadClips.Length - 1)];
                return reloadClip;
            }
        }

        [SerializeField]
        private AudioClip emptyClip;
        public AudioClip EmptyClip
        {
            get { return emptyClip; }
        }

        [SerializeField]
        private FireType fireType;
        public FireType FireType
        {
            get { return fireType; }
        }

        [SerializeField]
        private Vector2 fireStagger;
        public Vector2 FireStagger
        {
            get { return fireStagger; }
        }

        [SerializeField]
        private byte fireSpread;
        public byte FireSpread
        {
            get { return fireSpread; }
        }

        [SerializeField]
        private bool destroyImmediatelyAfterFired;
        public bool DestroyImmediatelyAfterFired
        {
            get { return destroyImmediatelyAfterFired; }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddDamageElements(DamageAmount);
            GameInstance.AddPoolingWeaponLaunchEffects(OffHandEquipmentModels);
            GameInstance.AddWeaponTypes(WeaponType);
        }
    }
}