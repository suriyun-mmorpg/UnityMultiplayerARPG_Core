using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct EquipmentRequirement
    {
        [FormerlySerializedAs("character")]
        [Tooltip("Which character classes can equip item. This is a part of `availableClasses`, just keep it for backward compatibility.")]
        public PlayerCharacter availableClass;
        [Tooltip("Which character classes can equip item.")]
        public List<PlayerCharacter> availableClasses;
        [Tooltip("Character must have level equals or more than this setting to equip item.")]
        public int level;
        [Tooltip("Character must have attribute amounts equals or more than this setting to equip item.")]
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributeAmounts;

        public bool HasAvailableClasses()
        {
            return availableClass != null || (availableClasses != null && availableClasses.Count > 0);
        }

        public bool ClassIsAvailable(PlayerCharacter characterClass)
        {
            if (!HasAvailableClasses())
                return true;
            if (availableClass != null && availableClass == characterClass)
                return true;
            if (availableClasses != null && availableClasses.Count > 0)
            {
                for (int i = 0; i < availableClasses.Count; ++i)
                {
                    if (availableClasses[i] != null && availableClasses[i] == characterClass)
                        return true;
                }
            }
            return false;
        }
    }
}
