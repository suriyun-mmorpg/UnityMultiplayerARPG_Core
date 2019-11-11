using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Attribute", menuName = "Create GameData/Attribute", order = -4997)]
    public partial class Attribute : BaseGameData
    {
        [Header("Attribute Configs")]
        public CharacterStats statsIncreaseEachLevel;
        [Tooltip("If this value more than 0 it will limit max amount of this attribute by this value")]
        public short maxAmount;

        public bool CanIncreaseAmount(IPlayerCharacterData character, short amount, out GameMessage.Type gameMessageType, bool checkStatPoint = true)
        {
            gameMessageType = GameMessage.Type.None;
            if (character == null)
                return false;

            if (maxAmount > 0 && amount >= maxAmount)
            {
                gameMessageType = GameMessage.Type.AttributeReachedMaxAmount;
                return false;
            }

            if (checkStatPoint && character.StatPoint <= 0)
            {
                gameMessageType = GameMessage.Type.NotEnoughStatPoint;
                return false;
            }

            return true;
        }
    }

    [System.Serializable]
    public struct AttributeAmount
    {
        public Attribute attribute;
        public float amount;
    }

    [System.Serializable]
    public struct AttributeIncremental
    {
        public Attribute attribute;
        public IncrementalFloat amount;
    }
}
