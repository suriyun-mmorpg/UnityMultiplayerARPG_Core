using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public abstract class BaseDamageEntity : RpgNetworkEntity
    {
        protected BaseCharacterEntity attacker;
        protected CharacterItem weapon;
        protected Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;
        protected CharacterBuff debuff;
        protected int hitEffectsId;

        public virtual void SetupDamage(
            BaseCharacterEntity attacker,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            CharacterBuff debuff,
            int hitEffectsId)
        {
            this.attacker = attacker;
            this.weapon = weapon;
            this.allDamageAmounts = allDamageAmounts;
            this.debuff = debuff;
            this.hitEffectsId = hitEffectsId;
        }

        public virtual void ApplyDamageTo(DamageableNetworkEntity target)
        {
            if (target == null)
                return;
            target.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
        }
    }
}
