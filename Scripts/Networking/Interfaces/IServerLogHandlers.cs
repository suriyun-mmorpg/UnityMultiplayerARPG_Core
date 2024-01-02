using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial interface IServerLogHandlers
    {
        void LogEnterGame(IPlayerCharacterData playerCharacter);
        void LogExitGame(IPlayerCharacterData playerCharacter);
        void LogRewardGiven(IPlayerCharacterData playerCharacter, RewardGivenType givenType, Reward reward);
        void LogItemGiven(IPlayerCharacterData playerCharacter, RewardGivenType givenType, CharacterItem item);

        void LogAttackStart(IPlayerCharacterData playerCharacter, int simulateSeed, float[] triggerDurations, byte fireSpread, bool isLeftHand, CharacterItem weapon);
        void LogAttackTrigger(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex);
        void LogAttackTriggerFail(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex, ActionTriggerFailReasons reason);
        void LogAttackInterrupt(IPlayerCharacterData playerCharacter, int simulateSeed);
        void LogAttackEnd(IPlayerCharacterData playerCharacter, int simulateSeed);

        void LogUseSkillStart(IPlayerCharacterData playerCharacter, int simulateSeed, float[] triggerDurations, byte fireSpread, bool isLeftHand, CharacterItem weapon, BaseSkill skill, int skillLevel);
        void LogUseSkillTrigger(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex);
        void LogUseSkillTriggerFail(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex, ActionTriggerFailReasons reason);
        void LogUseSkillInterrupt(IPlayerCharacterData playerCharacter, int simulateSeed);
        void LogUseSkillEnd(IPlayerCharacterData playerCharacter, int simulateSeed);

        void LogReloadStart(IPlayerCharacterData playerCharacter, float[] triggerDurations);
        void LogReloadTrigger(IPlayerCharacterData playerCharacter, byte triggerIndex);
        void LogReloadTriggerFail(IPlayerCharacterData playerCharacter, byte triggerIndex, ActionTriggerFailReasons reason);
        void LogReloadInterrupt(IPlayerCharacterData playerCharacter);
        void LogReloadEnd(IPlayerCharacterData playerCharacter);

        void LogChargeStart(IPlayerCharacterData playerCharacter);
        void LogChargeEnd(IPlayerCharacterData playerCharacter, bool willDoActionWhenStopCharging);

        void LogBuffApply(IPlayerCharacterData playerCharacter, CharacterBuff characterBuff);
        void LogBuffRemove(IPlayerCharacterData playerCharacter, CharacterBuff characterBuff, BuffRemoveReasons reason);

        void LogDamageReceived(IPlayerCharacterData playerCharacter, HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime);
        void LogKilled(IPlayerCharacterData playerCharacter, EntityInfo lastAttacker);
    }
}