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

        public void LogAttackTriggerFailNotEnoughResources(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex)
        {
            throw new System.NotImplementedException();
        }

        public void LogAttackTriggerFailNoValidateData(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex)
        {
            throw new System.NotImplementedException();
        }

        public void LogBuffApplied(IPlayerCharacterData playerCharacter, CharacterBuff buff)
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

        public void LogReloadTriggerFailNotEnoughResources(IPlayerCharacterData playerCharacter, byte triggerIndex)
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

        public void LogUseSkillTriggerFailNotEnoughResources(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex)
        {
            throw new System.NotImplementedException();
        }

        public void LogUseSkillTriggerFailNoValidateData(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex)
        {
            throw new System.NotImplementedException();
        }
    }
}
