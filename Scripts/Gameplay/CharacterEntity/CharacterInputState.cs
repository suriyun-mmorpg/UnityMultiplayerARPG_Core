namespace MultiplayerARPG
{
    [System.Flags]
    public enum CharacterInputState : ushort
    {
        None = 0,
        IsMoving = 1 << 0,
        IsAttacking = 1 << 1,
        IsUsingSkill = 1 << 2,
        IsUsingSkillItem = 1 << 3,
        IsReloading = 1 << 4,
        IsChargeStarting = 1 << 5,
        IsChargeStopping = 1 << 6,
    }

    public static class CharacterInputStateExtensions
    {
        public static bool Has(this CharacterInputState self, CharacterInputState flag)
        {
            return (self & flag) == flag;
        }
    }
}
