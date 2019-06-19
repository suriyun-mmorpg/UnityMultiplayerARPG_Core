namespace MultiplayerARPG
{
    public interface IVehicleAnims<TWeaponAnims, TSkillAnims>
        where TWeaponAnims : IWeaponAnims
        where TSkillAnims : ISkillAnims
    {
        VehicleType Data { get; }
        TWeaponAnims[] WeaponAnims { get; }
        TSkillAnims[] SkillAnims { get; }
    }
}
