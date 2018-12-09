using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public abstract class BaseDamageEntity : BaseGameEntity
    {
        protected IAttackerEntity attacker;
        protected CharacterItem weapon;
        protected Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;
        protected CharacterBuff debuff;
        protected uint hitEffectsId;

        public virtual void SetupDamage(
            IAttackerEntity attacker,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            CharacterBuff debuff,
            uint hitEffectsId)
        {
            this.attacker = attacker;
            this.weapon = weapon;
            this.allDamageAmounts = allDamageAmounts;
            this.debuff = debuff;
            this.hitEffectsId = hitEffectsId;
        }

        public virtual void ApplyDamageTo(IDamageableEntity target)
        {
            if (target == null)
                return;
            target.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
        }
    }
}
