using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public abstract class BaseDamageEntity : BaseGameEntity
    {
        public float destroyDelay;

        [SerializeField]
        protected SyncFieldInt skillId = new SyncFieldInt();

        protected IAttackerEntity attacker;
        protected CharacterItem weapon;
        protected Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;
        protected CharacterBuff debuff;

        protected Skill skill;
        public Skill Skill
        {
            get
            {
                if (skill == null && skillId.Value != 0)
                    GameInstance.Skills.TryGetValue(skillId.Value, out skill);
                return skill;
            }
            set
            {
                if (IsServer)
                    skillId.Value = value != null ? value.DataId : 0;
                skill = value;
            }
        }

        public virtual void SetupDamage(
            IAttackerEntity attacker,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            CharacterBuff debuff,
            Skill skill)
        {
            this.attacker = attacker;
            this.weapon = weapon;
            this.allDamageAmounts = allDamageAmounts;
            this.debuff = debuff;
            Skill = skill;
        }

        public virtual void ApplyDamageTo(IDamageableEntity target)
        {
            if (target == null)
                return;
            if (IsServer)
                target.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff);
            if (IsClient)
                target.PlayHitEffects(allDamageAmounts.Keys, skill);
        }
    }
}
