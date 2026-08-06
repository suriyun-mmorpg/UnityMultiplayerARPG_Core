using System;

namespace MultiplayerARPG
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CharacterStatTextGenAttribute : System.Attribute
    {
        public string Name { get; private set; }
        public bool IsRate { get; private set; }

        public CharacterStatTextGenAttribute(string name, bool isRate)
        {
            Name = name;
            IsRate = isRate;
        }
    }
}
