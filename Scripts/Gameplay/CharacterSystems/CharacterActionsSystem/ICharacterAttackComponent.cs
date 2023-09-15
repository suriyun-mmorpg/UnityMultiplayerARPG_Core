using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public interface ICharacterAttackComponent
    {
        bool IsAttacking { get; }
        float LastAttackEndTime { get; }
        float MoveSpeedRateWhileAttacking { get; }
        MovementRestriction MovementRestrictionWhileAttacking { get; }
        float AttackTotalDuration { get; set; }
        float[] AttackTriggerDurations { get; set; }

        void CancelAttack();
        void ClearAttackStates();
        void Attack(bool isLeftHand);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writeTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientAttackState(long writeTimestamp, NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writeTimestamp"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerAttackState(long writeTimestamp, NetDataWriter writer);
        void ReadClientAttackStateAtServer(long peerTimestamp, NetDataReader reader);
        void ReadServerAttackStateAtClient(long peerTimestamp, NetDataReader reader);
    }
}
