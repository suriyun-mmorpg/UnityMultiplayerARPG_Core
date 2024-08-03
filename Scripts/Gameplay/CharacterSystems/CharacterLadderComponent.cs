using LiteNetLibManager;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class CharacterLadderComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>
    {
        [SerializeField]
        private float overlapYOffsets = 1f;
        [SerializeField]
        private float overlapRadius = 0.5f;
        /// <summary>
        /// Triggered ladder entry, will decide to enter the ladder or not later
        /// </summary>
        public LadderEntry TriggeredLadderEntry { get; set; } = null;
        /// <summary>
        /// The ladder which the entity is climbing
        /// </summary>
        public Ladder ClimbingLadder { get; set; } = null;

        #region Play Enter Ladder Animation
        public void CallRpcPlayEnterLadderAnimation(LadderEntryType direction)
        {
            RPC(RpcPlayEnterLadderAnimation, direction);
        }

        [AllRpc]
        protected void RpcPlayEnterLadderAnimation(LadderEntryType direction)
        {
            PlayEnterLadderAnimation(direction);
        }

        public virtual void PlayEnterLadderAnimation(LadderEntryType direction)
        {
            // TODO: Implement this
        }
        #endregion

        #region Play Exit Ladder Animation
        public void CallRpcPlayExitLadderAnimation(LadderEntryType direction)
        {
            RPC(RpcPlayExitLadderAnimation, direction);
        }

        [AllRpc]
        protected void RpcPlayExitLadderAnimation(LadderEntryType direction)
        {
            PlayExitLadderAnimation(direction);
        }

        public virtual void PlayExitLadderAnimation(LadderEntryType direction)
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
            FindAndSetTriggeredLadderEntry();
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
            RpcPlayExitLadderAnimation(TriggeredLadderEntry.type);
            // TODO: Get exiting duration
            ClimbingLadder = null;
            RPC(TargetConfirmExitLadder, ConnectionId);
        }

        [TargetRpc]
        protected void TargetConfirmExitLadder()
        {
            ClimbingLadder = null;
        }

        public override void EntityUpdate()
        {
            if (!IsOwnerClient)
                return;
            FindAndSetTriggeredLadderEntry();
        }

        public void FindAndSetTriggeredLadderEntry()
        {
            Vector3 origin = Entity.EntityTransform.position + Entity.EntityTransform.up * overlapYOffsets;
            Collider[] results = Physics.OverlapSphere(origin, overlapRadius, GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask(), QueryTriggerInteraction.Collide);
            for (int i = 0; i < results.Length; ++i)
            {
                Collider collider = results[i];
                if (collider == null)
                    continue;
                if (!collider.TryGetComponent(out LadderEntry ladderEntry))
                    continue;
                TriggeredLadderEntry = ladderEntry;
                break;
            }
        }
    }
}