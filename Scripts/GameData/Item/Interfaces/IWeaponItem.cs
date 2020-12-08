using UnityEngine;

namespace MultiplayerARPG
{
    public partial interface IWeaponItem : IEquipmentItem
    {
        WeaponType WeaponType { get; }
        WeaponItemEquipType EquipType { get; }
        EquipmentModel[] OffHandEquipmentModels { get; }
        DamageIncremental DamageAmount { get; }
        IncrementalMinMaxFloat HarvestDamageAmount { get; }
        float MoveSpeedRateWhileAttacking { get; }
        short AmmoCapacity { get; }
        BaseWeaponAbility WeaponAbility { get; }
        CrosshairSetting CrosshairSetting { get; }
        AudioClip LaunchClip { get; }
        AudioClip ReloadClip { get; }
        AudioClip EmptyClip { get; }
        FireType FireType { get; }
        Vector2 FireStagger { get; }
        byte FireSpread { get; }
    }
}
