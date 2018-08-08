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
            RegisterNetFunction("StopMove", new LiteNetLibFunction(StopMove));
            RegisterNetFunction("SetTargetEntity", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncSetTargetEntity(objectId)));
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

        protected void NetFuncSetTargetEntity(uint objectId)
        {
            if (objectId == 0)
                SetTargetEntity(null);
            RpgNetworkEntity rpgNetworkEntity;
            if (!TryGetEntityByObjectId(objectId, out rpgNetworkEntity))
                return;
            SetTargetEntity(rpgNetworkEntity);
        }

        public override void KeyMovement(Vector3 direction, bool isJump)
        {
            if (IsDead())
                return;
            if (direction.magnitude <= 0.025f && !isJump)
                return;
            var position = CacheTransform.position + direction;
            if (IsOwnerClient && !IsServer && CacheNetTransform.ownerClientNotInterpolate)
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
            if (IsOwnerClient && !IsServer && CacheNetTransform.ownerClientNotInterpolate)
                SetMovePaths(position, true);
            CallNetFunction("PointClickMovement", FunctionReceivers.Server, position);
        }

        public override void StopMove()
        {
            base.StopMove();
            if (IsOwnerClient && !IsServer)
                CallNetFunction("StopMove", FunctionReceivers.Server);
        }

        public override void SetTargetEntity(RpgNetworkEntity entity)
        {
            base.SetTargetEntity(entity);
            if (IsOwnerClient && !IsServer)
                CallNetFunction("SetTargetEntity", FunctionReceivers.Server, entity == null ? 0 : entity.ObjectId);
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
