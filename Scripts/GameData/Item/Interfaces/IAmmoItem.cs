namespace MultiplayerARPG
{
    public partial interface IAmmoItem : IItem
    {
        /// <summary>
        /// Ammo type data
        /// </summary>
        AmmoType AmmoType { get; }
        /// <summary>
        /// Increasing damages stats while attacking by weapon which put this item
        /// </summary>
        DamageIncremental[] IncreaseDamages { get; }
    }
}
