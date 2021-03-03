using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IEntityMovement
    {
        BaseGameEntity Entity { get; }
        float StoppingDistance { get; }
        void StopMove();
        void KeyMovement(Vector3 moveDirection, MovementState movementState);
        void PointClickMovement(Vector3 position);
        void SetLookRotation(Quaternion rotation);
        Quaternion GetLookRotation();
        void Teleport(Vector3 position, Quaternion rotation);
        bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result);
    }

    public interface IEntityMovementComponent : IEntityMovement, IGameEntityComponent
    {
        void HandleSyncTransformAtClient(MessageHandlerData messageHandler);
        void HandleTeleportAtClient(MessageHandlerData messageHandler);
        void HandleKeyMovementAtServer(MessageHandlerData messageHandler);
        void HandlePointClickMovementAtServer(MessageHandlerData messageHandler);
        void HandleSetLookRotationAtServer(MessageHandlerData messageHandler);
        void HandleSyncTransformAtServer(MessageHandlerData messageHandler);
        void HandleTeleportAtServer(MessageHandlerData messageHandler);
        void HandleStopMoveAtServer(MessageHandlerData messageHandler);
    }
}
