using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class CharacterLadderComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>
    {
        /// <summary>
        /// Triggered ladder entry, will decide to enter the ladder or not later
        /// </summary>
        public LadderEntry TriggeredLadderEntry { get; set; } = null;
        /// <summary>
        /// The ladder which the entity is climbing
        /// </summary>
        public Ladder ClimbingLadder { get; set; } = null;

        #region Play Enter Ladder Animation
        public void CallRpcPlayEnterLadderAnimation(LadderEntryDirection direction)
        {
            RPC(RpcPlayEnterLadderAnimation, direction);
        }

        [AllRpc]
        protected void RpcPlayEnterLadderAnimation(LadderEntryDirection direction)
        {
            PlayEnterLadderAnimation(direction);
        }

        public virtual void PlayEnterLadderAnimation(LadderEntryDirection direction)
        {
            // TODO: Implement this
        }
        #endregion

        #region Play Exit Ladder Animation
        public void CallRpcPlayExitLadderAnimation(LadderEntryDirection direction)
        {
            RPC(RpcPlayExitLadderAnimation, direction);
        }

        [AllRpc]
        protected void RpcPlayExitLadderAnimation(LadderEntryDirection direction)
        {
            PlayExitLadderAnimation(direction);
        }

        public virtual void PlayExitLadderAnimation(LadderEntryDirection direction)
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
            RpcPlayEnterLadderAnimation(TriggeredLadderEntry.entryDirection);
            // TODO: Get entering duration
            ClimbingLadder = TriggeredLadderEntry.ladder;
            RPC(TargetConfirmEnterLadder);
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
            if (!TriggeredLadderEntry)
            {
                // No triggered ladder, so it cannot exit
                return;
            }
            if (!ClimbingLadder)
            {
                // Not climbing yet, do not exit
                return;
            }
            RpcPlayExitLadderAnimation(TriggeredLadderEntry.entryDirection);
            // TODO: Get exiting duration
            ClimbingLadder = null;
            RPC(TargetConfirmExitLadder);
        }

        [TargetRpc]
        protected void TargetConfirmExitLadder()
        {
            ClimbingLadder = null;
        }
    }
}