using System;
using UnityEngine;

namespace MultiplayerARPG
{
    [Serializable]
    public struct EquipmentEntityEffect : IComparable<EquipmentEntityEffect>
    {
        public int level;
        [HideInInspector]
        [Obsolete("This is deprecated, use `effectMaterials` instead.")]
        public Material[] materials;
        public MaterialCollection[] equipmentMaterials;
        public GameObject[] effectObjects;

        public int CompareTo(EquipmentEntityEffect other)
        {
            return level.CompareTo(other.level);
        }

        public void ApplyMaterials()
        {
            equipmentMaterials.ApplyMaterials();
        }
    }
}
