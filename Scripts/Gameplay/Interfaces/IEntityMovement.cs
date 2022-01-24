using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IEntityMovement
    {
        BaseGameEntity Entity { get; }
        float StoppingDistance { get; }
        MovementState MovementState { get; }
        ExtraMovementState ExtraMovementState { get; }
        DirectionVector2 Direction2D { get; set; }
        void StopMove();
        void KeyMovement(Vector3 moveDirection, MovementState movementState);
        void PointClickMovement(Vector3 position);
        void SetExtraMovementState(ExtraMovementState extraMovementState);
        void SetLookRotation(Quaternion rotation);
        Quaternion GetLookRotation();
        void Teleport(Vector3 position, Quaternion rotation);
        bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result);
    }

    public interface IEntityMovementComponent : IEntityMovement, IGameEntityComponent
    {
        void HandleSyncTransformAtClient(MessageHandlerData messageHandler);
        void HandleTeleportAtClient(MessageHandlerData messageHandler);
        void HandleJumpAtClient(MessageHandlerData messageHandler);
        void HandleMovementInputAtServer(MessageHandlerData messageHandler);
        void HandleSyncTransformAtServer(MessageHandlerData messageHandler);
        void HandleStopMoveAtServer(MessageHandlerData messageHandler);
        void HandleJumpAtServer(MessageHandlerData messageHandler);
    }
}
