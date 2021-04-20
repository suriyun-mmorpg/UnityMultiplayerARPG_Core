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
        float MoveSpeedRateWhileUsingSkill { get; }

        void InterruptCastingSkill();
        void CancelSkill();
        void ClearUseSkillStates();
        bool UseSkill(int dataId, bool isLeftHand, AimPosition aimPosition);
        bool UseSkillItem(short itemIndex, bool isLeftHand, AimPosition aimPosition);
    }
}
