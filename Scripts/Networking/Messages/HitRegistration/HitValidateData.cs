using System.Collections.Generic;

namespace MultiplayerARPG
{
    public struct HitValidateData
    {
        public byte FireSpread { get; set; }
        public Dictionary<DamageElement, MinMaxFloat> DamageAmounts { get; set; }
        public DamageInfo DamageInfo { get; set; }
        public CharacterItem Weapon { get; set; }
        public BaseSkill Skill { get; set; }
        public int SkillLevel { get; set; }
    }
}
