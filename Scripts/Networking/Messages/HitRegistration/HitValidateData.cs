using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class HitValidateData
    {
        public BaseGameEntity Attacker { get; set; }
        public float[] TriggerDurations { get; set; }
        public byte FireSpread { get; set; }
        public DamageInfo DamageInfo { get; set; }
        public Dictionary<DamageElement, MinMaxFloat> BaseDamageAmounts { get; set; }
        public bool IsLeftHand { get; set; }
        public CharacterItem Weapon { get; set; }
        public BaseSkill Skill { get; set; }
        public int SkillLevel { get; set; }
        public Dictionary<byte, Dictionary<DamageElement, MinMaxFloat>> ConfirmedDamageAmounts { get; } = new Dictionary<byte, Dictionary<DamageElement, MinMaxFloat>>();
        public Dictionary<string, int> HitsCount { get; } = new Dictionary<string, int>();
        public HashSet<string> HitObjects { get; } = new HashSet<string>();
    }
}
