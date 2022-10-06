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
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteClientAttackState(NetDataWriter writer);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        bool WriteServerAttackState(NetDataWriter writer);
        void ReadClientAttackStateAtServer(NetDataReader reader);
        void ReadServerAttackStateAtClient(NetDataReader reader);
    }
}
