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
        public void CallRpcConfirmEnterLadder(LadderEntranceType direction)
        {
            RPC(RpcConfirmEnterLadder, direction);
        }

        [AllRpc]
        protected void RpcConfirmEnterLadder(LadderEntranceType direction)
        {
            ClimbingLadder = TriggeredLadderEntry.ladder;
            PlayEnterLadderAnimation(direction);
        }

        public virtual void PlayEnterLadderAnimation(LadderEntranceType direction)
        {
            // TODO: Implement this
        }
        #endregion

        #region Play Exit Ladder Animation
        public void CallRpcConfirmExitLadder(LadderEntranceType direction)
        {
            RPC(RpcConfirmExitLadder, direction);
        }

        [AllRpc]
        protected void RpcConfirmExitLadder(LadderEntranceType direction)
        {
            ClimbingLadder = null;
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
            CallRpcConfirmEnterLadder(TriggeredLadderEntry.type);
        }

        public void CallCmdExitLadder(LadderEntranceType entranceType)
        {
            RPC(CmdExitLadder, entranceType);
        }

        [ServerRpc]
        protected void CmdExitLadder(LadderEntranceType entranceType)
        {
            ExitLadder(entranceType);
        }

        public void ExitLadder(LadderEntranceType entranceType)
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
            CallRpcConfirmExitLadder(entranceType);
        }
    }
}