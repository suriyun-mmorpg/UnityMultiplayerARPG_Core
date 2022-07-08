using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class CharacterSkillAndBuffComponent : BaseGameEntityComponent<BaseCharacterEntity>
    {
        public const float SKILL_BUFF_UPDATE_DURATION = 1f;

        private float updatingTime;
        private float deltaTime;
        private Dictionary<string, CharacterRecoveryData> recoveryBuffs;

        public override void EntityStart()
        {
            recoveryBuffs = new Dictionary<string, CharacterRecoveryData>();
        }

        public override sealed void EntityUpdate()
        {
            if (!Entity.IsServer)
                return;

            deltaTime = Time.unscaledDeltaTime;
            updatingTime += deltaTime;

            if (Entity.IsRecaching || Entity.IsDead())
                return;

            if (updatingTime >= SKILL_BUFF_UPDATE_DURATION)
            {
                // Removing summons if it should
                int count = Entity.Summons.Count;
                CharacterSummon summon;
                for (int i = count - 1; i >= 0; --i)
                {
                    summon = Entity.Summons[i];
                    if (summon.ShouldRemove())
                    {
                        Entity.Summons.RemoveAt(i);
                        summon.UnSummon(Entity);
                    }
                    else
                    {
                        summon.Update(updatingTime);
                        Entity.Summons[i] = summon;
                    }
                }
                // Removing skill usages if it should
                count = Entity.SkillUsages.Count;
                CharacterSkillUsage skillUsage;
                for (int i = count - 1; i >= 0; --i)
                {
                    skillUsage = Entity.SkillUsages[i];
                    if (skillUsage.ShouldRemove())
                    {
                        Entity.SkillUsages.RemoveAt(i);
                    }
                    else
                    {
                        skillUsage.Update(updatingTime);
                        Entity.SkillUsages[i] = skillUsage;
                    }
                }
                // Removing buffs if it should
                count = Entity.Buffs.Count;
                CharacterBuff buff;
                float duration;
                for (int i = count - 1; i >= 0; --i)
                {
                    buff = Entity.Buffs[i];
                    duration = buff.GetDuration();
                    if (buff.ShouldRemove())
                    {
                        recoveryBuffs.Remove(buff.id);
                        Entity.Buffs.RemoveAt(i);
                    }
                    else
                    {
                        buff.Update(updatingTime);
                        Entity.Buffs[i] = buff;
                    }
                    // If duration is 0, damages / recoveries will applied immediately, so don't apply it here
                    if (duration > 0f)
                    {
                        CharacterRecoveryData recoveryData;
                        if (!recoveryBuffs.TryGetValue(buff.id, out recoveryData))
                        {
                            recoveryData = new CharacterRecoveryData(Entity);
                            recoveryData.SetupByBuff(buff);
                            recoveryBuffs.Add(buff.id, recoveryData);
                        }
                        recoveryData.Apply(1 / duration * updatingTime);
                    }
                    // Don't update next buffs if character dead
                    if (Entity.IsDead())
                        break;
                }
                updatingTime = 0;
            }
        }
    }
}
