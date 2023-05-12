using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class HitValidateData
    {
        public BaseGameEntity Attacker { get; set; }
        public float[] TriggerDurations { get; set; }
        public byte FireSpread { get; set; }
        public DamageInfo DamageInfo { get; set; }
        public Dictionary<DamageElement, MinMaxFloat> DamageAmounts { get; set; }
        public CharacterItem Weapon { get; set; }
        public BaseSkill Skill { get; set; }
        public int SkillLevel { get; set; }
        public List<HitOriginData>[] Origins { get; set; }
        public Dictionary<string, List<HitData>> Hits { get; } = new Dictionary<string, List<HitData>>();
    }
}
