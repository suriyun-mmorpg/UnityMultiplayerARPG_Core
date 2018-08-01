using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody))]
    public class SecurePlayerCharacterEntity : PlayerCharacterEntity
    {
        public override void OnSetup()
        {
            base.OnSetup();
            // Setup network components
            CacheNetTransform.ownerClientCanSendTransform = false;
            CacheNetTransform.ownerClientNotInterpolate = true;
            // Register Network functions
            RegisterNetFunction("PointClickMovement", new LiteNetLibFunction<NetFieldVector3>((position) => NetFuncPointClickMovement(position)));
            RegisterNetFunction("KeyMovement", new LiteNetLibFunction<NetFieldVector3, NetFieldBool>((position, isJump) => NetFuncKeyMovement(position, isJump)));
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            SetMovePaths(position, true);
            currentNpcDialog = null;
        }

        protected void NetFuncKeyMovement(Vector3 position, bool isJump)
        {
            if (IsDead())
                return;
            SetMovePaths(position, false);
            if (!isJumping)
                isJumping = isGrounded && isJump;
            currentNpcDialog = null;
        }

        public override void KeyMovement(Vector3 direction, bool isJump)
        {
            if (IsDead())
                return;
            if (direction.magnitude <= 0.025f && !isJump)
                return;
            var position = CacheTransform.position + direction;
            if (!IsServer && CacheNetTransform.ownerClientNotInterpolate)
            {
                SetMovePaths(position, false);
                if (!isJumping)
                    isJumping = isGrounded && isJump;
            }
            CallNetFunction("KeyMovement", FunctionReceivers.Server, position, isJump);
        }

        public override void PointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            if (!IsServer && CacheNetTransform.ownerClientNotInterpolate)
                SetMovePaths(position, true);
            CallNetFunction("PointClickMovement", FunctionReceivers.Server, position);
        }

        public override void RequestTriggerJump()
        {
            if (IsDead())
                return;
            // Play jump animation immediately on owner client
            if (IsOwnerClient)
                CharacterModel.PlayJumpAnimation();
            // Only server will call for clients to trigger jump animation for secure entity
            if (IsServer)
                CallNetFunction("TriggerJump", FunctionReceivers.All);
        }
    }
}
