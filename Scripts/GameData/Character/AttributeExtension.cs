using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class AttributeExtension
    {
        public static CharacterStats GetStats(this Attribute attribute, float level)
        {
            if (attribute == null)
                return new CharacterStats();
            return attribute.statsIncreaseEachLevel * level;
        }

        public static CharacterStats GetStats(this AttributeAmount attributeAmount)
        {
            if (attributeAmount.attribute == null)
                return new CharacterStats();
            Attribute attribute = attributeAmount.attribute;
            return attribute.GetStats(attributeAmount.amount);
        }

        public static CharacterStats GetStats(this AttributeIncremental attributeIncremental, short level)
        {
            if (attributeIncremental.attribute == null)
                return new CharacterStats();
            Attribute attribute = attributeIncremental.attribute;
            return attribute.GetStats(attributeIncremental.amount.GetAmount(level));
        }

        public static bool CanIncreaseAmount(this Attribute attribute, IPlayerCharacterData character, short amount, out GameMessage.Type gameMessageType, bool checkStatPoint = true)
        {
            gameMessageType = GameMessage.Type.None;
            if (attribute == null || character == null)
                return false;

            if (attribute.maxAmount > 0 && amount >= attribute.maxAmount)
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
}
