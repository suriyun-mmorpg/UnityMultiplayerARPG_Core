namespace MultiplayerARPG
{
    public enum WeaponAbilityState
    {
        Deactivated,
        Activated,
        Deactivating,
        Activating,
    }

    public static class WeaponAbilityStateExtensions
    {
        public static bool IsActivate(this WeaponAbilityState self)
        {
            return self == WeaponAbilityState.Activating ||
                self == WeaponAbilityState.Activated;
        }

        public static bool IsDeactivate(this WeaponAbilityState self)
        {
            return self == WeaponAbilityState.Deactivating ||
                self == WeaponAbilityState.Deactivated;
        }
    }
}
