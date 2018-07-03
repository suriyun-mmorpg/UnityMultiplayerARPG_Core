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
        private Vector3? lastClickPosition;
        public override void OnSetup()
        {
            base.OnSetup();
            // Setup network components
            CacheNetTransform.ownerClientCanSendTransform = false;
            CacheNetTransform.ownerClientNotInterpolate = true;
            // Register Network functions
            RegisterNetFunction("PointClickMovement", new LiteNetLibFunction<NetFieldVector3>((position) => NetFuncPointClickMovement(position)));
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            SetMovePaths(position);
            currentNpcDialog = null;
        }

        public void RequestPointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            if (!IsServer && CacheNetTransform.ownerClientNotInterpolate)
                SetMovePaths(position);
            CallNetFunction("PointClickMovement", FunctionReceivers.Server, position);
        }

        public override void KeyMovement(Vector3 direction, bool isJump)
        {
            if (IsDead())
                return;
            if (direction.magnitude <= 0.05f)
                return;
            RequestPointClickMovement(CacheTransform.position + direction);
        }

        public override void PointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            if (lastClickPosition.HasValue && lastClickPosition.Value == position)
                return;
            lastClickPosition = position;
            RequestPointClickMovement(position);
        }
    }
}
