using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class CharacterLadderComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>
    {
        protected LadderEntrance _triggeredLadderEntry;
        protected int _lastTriggeredLadderEntryFrame;
        /// <summary>
        /// Triggered ladder entry, will decide to enter the ladder or not later
        /// </summary>
        public LadderEntrance TriggeredLadderEntry
        {
            get
            {
                if (Time.frameCount > _lastTriggeredLadderEntryFrame)
                    _triggeredLadderEntry = null;
                return _triggeredLadderEntry;
            }
            set
            {
                _lastTriggeredLadderEntryFrame = Time.frameCount;
                _triggeredLadderEntry = value;
            }
        }
        /// <summary>
        /// The ladder which the entity is climbing
        /// </summary>
        public Ladder ClimbingLadder { get; set; } = null;

        #region Play Enter Ladder Animation
        public void CallRpcPlayEnterLadderAnimation(LadderEntranceType direction)
        {
            RPC(RpcPlayEnterLadderAnimation, direction);
        }

        [AllRpc]
        protected void RpcPlayEnterLadderAnimation(LadderEntranceType direction)
        {
            PlayEnterLadderAnimation(direction);
        }

        public virtual void PlayEnterLadderAnimation(LadderEntranceType direction)
        {
            // TODO: Implement this
        }
        #endregion

        #region Play Exit Ladder Animation
        public void CallRpcPlayExitLadderAnimation(LadderEntranceType direction)
        {
            RPC(RpcPlayExitLadderAnimation, direction);
        }

        [AllRpc]
        protected void RpcPlayExitLadderAnimation(LadderEntranceType direction)
        {
            PlayExitLadderAnimation(direction);
        }

        public virtual void PlayExitLadderAnimation(LadderEntranceType direction)
        {
            // TODO: Implement this
        }
        #endregion

        public void CallCmdEnterLadder()
        {
            RPC(CmdEnterLadder);
        }

        [ServerRpc]
        protected void CmdEnterLadder()
        {
            EnterLadder();
        }

        public void EnterLadder()
        {
            if (!IsServer)
            {
                Logging.LogWarning(LogTag, "Only server can perform ladder entering");
                return;
            }
            if (!TriggeredLadderEntry)
            {
                // No triggered ladder, so it cannot enter
                return;
            }
            if (ClimbingLadder && ClimbingLadder == TriggeredLadderEntry.ladder)
            {
                // Already climbing, do not enter
                return;
            }
            RpcPlayEnterLadderAnimation(TriggeredLadderEntry.type);
            // TODO: Get entering duration
            ClimbingLadder = TriggeredLadderEntry.ladder;
            RPC(TargetConfirmEnterLadder, ConnectionId);
        }

        [TargetRpc]
        protected void TargetConfirmEnterLadder()
        {
            ClimbingLadder = TriggeredLadderEntry.ladder;
        }

        public void CallCmdExitLadder()
        {
            RPC(CmdExitLadder);
        }

        [ServerRpc]
        protected void CmdExitLadder()
        {
            ExitLadder();
        }

        public void ExitLadder()
        {
            if (!IsServer)
            {
                Logging.LogWarning(LogTag, "Only server can perform ladder exiting");
                return;
            }
            if (!ClimbingLadder)
            {
                // Not climbing yet, do not exit
                return;
            }
            RpcPlayExitLadderAnimation(LadderEntranceType.Bottom);
            // TODO: Get exiting duration
            ClimbingLadder = null;
            RPC(TargetConfirmExitLadder, ConnectionId);
        }

        [TargetRpc]
        protected void TargetConfirmExitLadder()
        {
            ClimbingLadder = null;
        }
    }
}