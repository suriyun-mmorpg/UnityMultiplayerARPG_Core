using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerLogHandlers : MonoBehaviour, IServerLogHandlers
    {
        public void LogAttackEnd(IPlayerCharacterData playerCharacter, int simulateSeed)
        {
            throw new System.NotImplementedException();
        }

        public void LogAttackInterrupt(IPlayerCharacterData playerCharacter, int simulateSeed)
        {
            throw new System.NotImplementedException();
        }

        public void LogAttackStart(IPlayerCharacterData playerCharacter, int simulateSeed, float[] triggerDurations, byte fireSpread, bool isLeftHand, CharacterItem weapon)
        {
            throw new System.NotImplementedException();
        }

        public void LogAttackTrigger(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex)
        {
            throw new System.NotImplementedException();
        }

        public void LogAttackTriggerFail(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex, ActionTriggerFailReasons reason)
        {
            throw new System.NotImplementedException();
        }

        public void LogBuffApply(IPlayerCharacterData playerCharacter, CharacterBuff characterBuff)
        {
            throw new System.NotImplementedException();
        }

        public void LogBuffRemove(IPlayerCharacterData playerCharacter, CharacterBuff characterBuff, BuffRemoveReasons reason)
        {
            throw new System.NotImplementedException();
        }

        public void LogChargeEnd(IPlayerCharacterData playerCharacter, bool willDoActionWhenStopCharging)
        {
            throw new System.NotImplementedException();
        }

        public void LogChargeStart(IPlayerCharacterData playerCharacter)
        {
            throw new System.NotImplementedException();
        }

        public void LogDamageReceived(IPlayerCharacterData playerCharacter, HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime)
        {
            throw new System.NotImplementedException();
        }

        public void LogEnterGame(IPlayerCharacterData playerCharacter)
        {
            throw new System.NotImplementedException();
        }

        public void LogExitGame(IPlayerCharacterData playerCharacter)
        {
            throw new System.NotImplementedException();
        }

        public void LogItemGiven(IPlayerCharacterData playerCharacter, RewardGivenType givenType, CharacterItem item)
        {
            throw new System.NotImplementedException();
        }

        public void LogKilled(IPlayerCharacterData playerCharacter, EntityInfo lastAttacker)
        {
            throw new System.NotImplementedException();
        }

        public void LogReloadEnd(IPlayerCharacterData playerCharacter)
        {
            throw new System.NotImplementedException();
        }

        public void LogReloadInterrupt(IPlayerCharacterData playerCharacter)
        {
            throw new System.NotImplementedException();
        }

        public void LogReloadStart(IPlayerCharacterData playerCharacter, float[] triggerDurations)
        {
            throw new System.NotImplementedException();
        }

        public void LogReloadTrigger(IPlayerCharacterData playerCharacter, byte triggerIndex)
        {
            throw new System.NotImplementedException();
        }

        public void LogReloadTriggerFail(IPlayerCharacterData playerCharacter, byte triggerIndex, ActionTriggerFailReasons reason)
        {
            throw new System.NotImplementedException();
        }

        public void LogRewardGiven(IPlayerCharacterData playerCharacter, RewardGivenType givenType, Reward reward)
        {
            throw new System.NotImplementedException();
        }

        public void LogUseSkillEnd(IPlayerCharacterData playerCharacter, int simulateSeed)
        {
            throw new System.NotImplementedException();
        }

        public void LogUseSkillInterrupt(IPlayerCharacterData playerCharacter, int simulateSeed)
        {
            throw new System.NotImplementedException();
        }

        public void LogUseSkillStart(IPlayerCharacterData playerCharacter, int simulateSeed, float[] triggerDurations, byte fireSpread, bool isLeftHand, CharacterItem weapon, BaseSkill skill, int skillLevel)
        {
            throw new System.NotImplementedException();
        }

        public void LogUseSkillTrigger(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex)
        {
            throw new System.NotImplementedException();
        }

        public void LogUseSkillTriggerFail(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex, ActionTriggerFailReasons reason)
        {
            throw new System.NotImplementedException();
        }
    }
}
