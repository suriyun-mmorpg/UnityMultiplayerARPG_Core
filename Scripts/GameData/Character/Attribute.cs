using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Attribute", menuName = "Create GameData/Attribute")]
    public partial class Attribute : BaseGameData
    {
        public CharacterStats statsIncreaseEachLevel;
        [Tooltip("If this value more than 0 it will limit max amount of this attribute by this value")]
        public short maxAmount;
    }

    [System.Serializable]
    public struct AttributeAmount
    {
        public Attribute attribute;
        public short amount;
    }

    [System.Serializable]
    public struct AttributeIncremental
    {
        public Attribute attribute;
        public IncrementalShort amount;
    }
}
