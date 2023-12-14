namespace MultiplayerARPG
{
    public interface ICharacterUseSkillComponent
    {
        BaseSkill UsingSkill { get; }
        int UsingSkillLevel { get; }
        bool IsCastingSkillCanBeInterrupted { get; }
        bool IsCastingSkillInterrupted { get; }
        float CastingSkillDuration { get; }
        float CastingSkillCountDown { get; }
        bool IsUsingSkill { get; }
        float LastUseSkillEndTime { get; }
        bool IsSkipMovementValidationWhileUsingSkill { get; }
        bool IsUseRootMotionWhileUsingSkill { get; }
        float MoveSpeedRateWhileUsingSkill { get; }
        MovementRestriction MovementRestrictionWhileUsingSkill { get; }
        float UseSkillTotalDuration { get; set; }
        float[] UseSkillTriggerDurations { get; set; }

        void InterruptCastingSkill();
        void CancelSkill();
        void ClearUseSkillStates();
        void UseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition);
        void UseSkillItem(int itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition);
    }
}
