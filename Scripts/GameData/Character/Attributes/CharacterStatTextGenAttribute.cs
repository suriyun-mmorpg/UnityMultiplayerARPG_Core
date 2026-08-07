using System;

namespace MultiplayerARPG
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CharacterStatTextGenAttribute : System.Attribute
    {
        public string FormatKey { get; private set; }
        public bool IsRate { get; private set; }

        public CharacterStatTextGenAttribute(string formatKey, bool isRate)
        {
            FormatKey = formatKey;
            IsRate = isRate;
        }

        public CharacterStatTextGenAttribute(bool isRate) : this(string.Empty, isRate)
        {
        }
    }
}
