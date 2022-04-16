using LiteNetLib.Utils;

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
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientUseSkillState(NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerUseSkillState(NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientUseSkillItemState(NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerUseSkillItemState(NetDataWriter writer);
        void ReadClientUseSkillStateAtServer(NetDataReader reader);
        void ReadServerUseSkillStateAtClient(NetDataReader reader);
        void ReadClientUseSkillItemStateAtServer(NetDataReader reader);
        void ReadServerUseSkillItemStateAtClient(NetDataReader reader);
    }
}
