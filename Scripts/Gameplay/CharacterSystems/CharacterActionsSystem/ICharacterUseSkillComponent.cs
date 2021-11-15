namespace MultiplayerARPG
{
    public interface ICharacterUseSkillComponent
    {
        BaseSkill UsingSkill { get; }
        short UsingSkillLevel { get; }
        bool IsCastingSkillCanBeInterrupted { get; }
        bool IsCastingSkillInterrupted { get; }
        float CastingSkillDuration { get; }
        float CastingSkillCountDown { get; }
        bool IsUsingSkill { get; }
        float LastUseSkillEndTime { get; }
        float MoveSpeedRateWhileUsingSkill { get; }

        void InterruptCastingSkill();
        void CancelSkill();
        void ClearUseSkillStates();
        void UseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition);
        void UseSkillItem(short itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition);
    }
}
