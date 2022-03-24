using System.Collections.Generic;

namespace MultiplayerARPG
{
    public struct HitValidateData
    {
        public byte FireSpread { get; set; }
        public BaseCharacterEntity Attacker { get; set; }
        public Dictionary<DamageElement, MinMaxFloat> DamageAmounts { get; set; }
        public CharacterItem Weapon { get; set; }
        public BaseSkill Skill { get; set; }
        public short SkillLevel { get; set; }
    }
}
