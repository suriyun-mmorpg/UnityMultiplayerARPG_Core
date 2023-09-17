using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HarvestEffectiveness
    {
        public WeaponType weaponType;
        [Tooltip("This will multiply with harvest damage amount")]
        [Range(0.1f, 5f)]
        public float damageEffectiveness;
        [ArrayElementTitle("item")]
        public ItemDropForHarvestable[] items;
    }
}