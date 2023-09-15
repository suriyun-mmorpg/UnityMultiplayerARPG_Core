using LiteNetLib.Utils;

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
        float MoveSpeedRateWhileUsingSkill { get; }
        MovementRestriction MovementRestrictionWhileUsingSkill { get; }
        float UseSkillTotalDuration { get; set; }
        float[] UseSkillTriggerDurations { get; set; }

        void InterruptCastingSkill();
        void CancelSkill();
        void ClearUseSkillStates();
        void UseSkill(int dataId, bool isLeftHand, uint targetObjectId, AimPosition aimPosition);
        void UseSkillItem(int itemIndex, bool isLeftHand, uint targetObjectId, AimPosition aimPosition);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writerTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientUseSkillState(long writerTimestamp, NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writerTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerUseSkillState(long writerTimestamp, NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writerTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientUseSkillItemState(long writerTimestamp, NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writerTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerUseSkillItemState(long writerTimestamp, NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writerTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientUseSkillInterruptedState(long writerTimestamp, NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writerTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerUseSkillInterruptedState(long writerTimestamp, NetDataWriter writer);
        void ReadClientUseSkillStateAtServer(long peerTimestamp, NetDataReader reader);
        void ReadServerUseSkillStateAtClient(long peerTimestamp, NetDataReader reader);
        void ReadClientUseSkillItemStateAtServer(long peerTimestamp, NetDataReader reader);
        void ReadServerUseSkillItemStateAtClient(long peerTimestamp, NetDataReader reader);
        void ReadClientUseSkillInterruptedStateAtServer(long peerTimestamp, NetDataReader reader);
        void ReadServerUseSkillInterruptedStateAtClient(long peerTimestamp, NetDataReader reader);
    }
}
