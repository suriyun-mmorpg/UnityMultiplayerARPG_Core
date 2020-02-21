namespace MultiplayerARPG
{
    public partial interface IAmmoItem : IItem
    {
        AmmoType AmmoType { get; }
        DamageIncremental[] IncreaseDamages { get; }
    }
}
