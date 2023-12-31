namespace MultiplayerARPG
{
    public partial interface IServerLogHandlers
    {
        void LogEnterGame(IPlayerCharacterData playerCharacter);
        void LogExitGame(IPlayerCharacterData playerCharacter);
        void LogRewardGiven(IPlayerCharacterData playerCharacter, RewardGivenType givenType, Reward reward);
        void LogItemGiven(IPlayerCharacterData playerCharacter, RewardGivenType givenType, CharacterItem item);

        void LogAttackStart(IPlayerCharacterData playerCharacter);
        void LogAttackTrigger(IPlayerCharacterData playerCharacter);
        void LogAttackInterrupt(IPlayerCharacterData playerCharacter);
        void LogAttackEnd(IPlayerCharacterData playerCharacter);

        void LogUseSkillStart(IPlayerCharacterData playerCharacter);
        void LogUseSkillTrigger(IPlayerCharacterData playerCharacter);
        void LogUseSkillInterrupt(IPlayerCharacterData playerCharacter);
        void LogUseSkillEnd(IPlayerCharacterData playerCharacter);

        void LogUseReloadStart(IPlayerCharacterData playerCharacter);
        void LogUseReloadTrigger(IPlayerCharacterData playerCharacter);
        void LogUseReloadInterrupt(IPlayerCharacterData playerCharacter);
        void LogUseReloadEnd(IPlayerCharacterData playerCharacter);

        void LogUseChargeStart(IPlayerCharacterData playerCharacter);
        void LogUseChargeTrigger(IPlayerCharacterData playerCharacter);
        void LogUseChargeInterrupt(IPlayerCharacterData playerCharacter);
        void LogUseChargeEnd(IPlayerCharacterData playerCharacter);

        void LogBuffApplied(IPlayerCharacterData playerCharacter, CharacterBuff buff);
    }
}