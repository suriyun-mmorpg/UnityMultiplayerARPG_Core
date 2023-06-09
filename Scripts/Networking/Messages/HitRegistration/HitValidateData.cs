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
        public Dictionary<string, HitOriginData> Origins { get; } = new Dictionary<string, HitOriginData>();
        public Dictionary<string, int> HitsCount { get; } = new Dictionary<string, int>();
        public HashSet<string> HitObjects { get; } = new HashSet<string>();
        public Dictionary<string, HitData> Pendings { get; } = new Dictionary<string, HitData>();
    }
}
