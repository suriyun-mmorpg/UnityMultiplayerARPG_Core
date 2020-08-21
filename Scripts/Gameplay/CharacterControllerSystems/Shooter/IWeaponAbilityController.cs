using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IWeaponAbilityController : IShooterWeaponController
    {
        BaseWeaponAbility WeaponAbility { get; }
        WeaponAbilityState WeaponAbilityState { get; }
    }
}
