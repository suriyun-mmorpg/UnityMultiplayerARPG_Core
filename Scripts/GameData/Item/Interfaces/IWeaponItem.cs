using UnityEngine;

namespace MultiplayerARPG
{
    public partial interface IWeaponItem : IEquipmentItem
    {
        /// <summary>
        /// Weapon type data
        /// </summary>
        WeaponType WeaponType { get; }
        /// <summary>
        /// Off-hand equipment models, these models will be instantiated when equipping this item to off-hand (left-hand)
        /// </summary>
        EquipmentModel[] OffHandEquipmentModels { get; }
        /// <summary>
        /// Damange amount which will be used when attacking characters, buildings and so on
        /// </summary>
        DamageIncremental DamageAmount { get; }
        /// <summary>
        /// Damage amount which will be used when attacking harvestable entities
        /// </summary>
        IncrementalMinMaxFloat HarvestDamageAmount { get; }
        /// <summary>
        /// This will be multiplied with character's movement speed while reloading this weapon
        /// </summary>
        float MoveSpeedRateWhileReloading { get; }
        MovementRestriction MovementRestrictionWhileReloading { get; }
        /// <summary>
        /// This will be multiplied with character's movement speed while charging this weapon
        /// </summary>
        float MoveSpeedRateWhileCharging { get; }
        MovementRestriction MovementRestrictionWhileCharging { get; }
        /// <summary>
        /// This will be multiplied with character's movement speed while attacking with this weapon
        /// </summary>
        float MoveSpeedRateWhileAttacking { get; }
        MovementRestriction MovementRestrictionWhileAttacking { get; }
        /// <summary>
        /// How many bullets can store in the gun
        /// </summary>
        short AmmoCapacity { get; }
        /// <summary>
        /// Weapon ability such as zoom, change how to fire, change launch clip. (For now, it has only zoom)
        /// </summary>
        BaseWeaponAbility WeaponAbility { get; }
        /// <summary>
        /// Crosshair setting
        /// </summary>
        CrosshairSetting CrosshairSetting { get; }
        /// <summary>
        /// Audio clip which will plays when fire and bullet launched
        /// </summary>
        AudioClip LaunchClip { get; }
        /// <summary>
        /// Audio clip which will plays when reload an ammo
        /// </summary>
        AudioClip ReloadClip { get; }
        /// <summary>
        /// Audio clip which will plays when there is no ammo
        /// </summary>
        AudioClip EmptyClip { get; }
        /// <summary>
        /// How to fire
        /// </summary>
        FireType FireType { get; }
        /// <summary>
        /// Random stagger from aiming position, then when shoot actual shot position will be {aim position} + {randomed stagger}
        /// </summary>
        Vector2 FireStagger { get; }
        /// <summary>
        /// Amount of bullets that will be launched when fire onnce, will be used for shotgun items
        /// </summary>
        byte FireSpread { get; }
        /// <summary>
        /// If this is `TRUE`, character's item will be destroyed after fired, will be used for grenade items
        /// </summary>
        bool DestroyImmediatelyAfterFired { get; }
    }
}
