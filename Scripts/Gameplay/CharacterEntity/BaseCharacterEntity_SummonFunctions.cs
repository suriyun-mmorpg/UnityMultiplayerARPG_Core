using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected virtual void ApplySkillSummon(Skill skill, short level)
        {
            if (IsDead() || !IsServer || skill == null || level <= 0)
                return;
            int i = 0;
            int amountEachTime = skill.summon.amountEachTime.GetAmount(level);
            for (i = 0; i < amountEachTime; ++i)
            {
                CharacterSummon newSummon = CharacterSummon.Create(SummonType.Skill, skill.DataId);
                newSummon.Summon(this, skill.summon.level.GetAmount(level), skill.summon.duration.GetAmount(level));
                summons.Add(newSummon);
            }
            int count = 0;
            for (i = 0; i < summons.Count; ++i)
            {
                if (summons[i].dataId == skill.DataId)
                    ++count;
            }
            int maxStack = skill.summon.maxStack.GetAmount(level);
            int unSummonAmount = count > maxStack ? count - maxStack : 0;
            CharacterSummon tempSummon;
            for (i = unSummonAmount; i > 0; --i)
            {
                int summonIndex = this.IndexOfSummon(skill.DataId, SummonType.Skill);
                tempSummon = summons[summonIndex];
                if (summonIndex >= 0)
                {
                    summons.RemoveAt(summonIndex);
                    tempSummon.UnSummon(this);
                }
            }
        }
    }
}
