using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameEntity
    {
        public const float GROUND_DETECTION_DISTANCE = 100f;

        [Category(3, "Entity Movement")]
        [SerializeField]
        protected bool canSideSprint = false;
        public bool CanSideSprint { get { return canSideSprint; } }

        [SerializeField]
        protected bool canBackwardSprint = false;
        public bool CanBackwardSprint { get { return canBackwardSprint; } }

        public IEntityMovementComponent Movement { get; private set; }

        public Transform MovementTransform
        {
            get
            {
                if (!PassengingVehicleEntity.IsNull())
                {
                    // Track movement position by vehicle entity
                    return PassengingVehicleEntity.Entity.EntityTransform;
                }
                return EntityTransform;
            }
        }

        public IEntityMovementComponent ActiveMovement
        {
            get
            {
                if (!PassengingVehicleEntity.IsNull())
                    return PassengingVehicleEntity.Entity.Movement;
                return Movement;
            }
        }

        public float StoppingDistance
        {
            get
            {
                return ActiveMovement.IsNull() ? 0.1f : ActiveMovement.StoppingDistance;
            }
        }

        public MovementState MovementState
        {
            get
            {
                return ActiveMovement.IsNull() ? MovementState.IsGrounded : ActiveMovement.MovementState;
            }
        }

        public ExtraMovementState ExtraMovementState
        {
            get
            {
                return ActiveMovement.IsNull() ? ExtraMovementState.None : ActiveMovement.ExtraMovementState;
            }
        }

        public DirectionVector2 Direction2D
        {
            get
            {
                return ActiveMovement.IsNull() ? (DirectionVector2)Vector2.down : ActiveMovement.Direction2D;
            }
            set
            {
                if (!ActiveMovement.IsNull())
                    ActiveMovement.Direction2D = value;
            }
        }

        public float CurrentMoveSpeed
        {
            get
            {
                return ActiveMovement.IsNull() ? 0f : ActiveMovement.CurrentMoveSpeed;
            }
        }

        public virtual bool SkipMovementValidation
        {
            get { return false; }
        }

        public float GetMoveSpeed()
        {
            return GetMoveSpeed(MovementState, ExtraMovementState);
        }

        public virtual float GetMoveSpeed(MovementState movementState, ExtraMovementState extraMovementState)
        {
            return 0f;
        }

        public float GetJumpHeight()
        {
            return GetJumpHeight(MovementState, ExtraMovementState);
        }

        public virtual float GetJumpHeight(MovementState movementState, ExtraMovementState extraMovementState)
        {
            return 0f;
        }

        public float GetGravityRate()
        {
            return GetGravityRate(MovementState, ExtraMovementState);
        }

        public virtual float GetGravityRate(MovementState movementState, ExtraMovementState extraMovementState)
        {
            return 1f;
        }

        public bool CanMove()
        {
            bool canMove = CanMove_Implementation();
            if (onCanMoveValidated != null)
                onCanMoveValidated(ref canMove);
            return canMove;
        }

        protected virtual bool CanMove_Implementation()
        {
            return false;
        }

        public bool CanSprint()
        {
            bool canSprint = CanSprint_Implementation();
            if (onCanSprintValidated != null)
                onCanSprintValidated(ref canSprint);
            return canSprint;
        }

        protected virtual bool CanSprint_Implementation()
        {
            return false;
        }

        public bool CanWalk()
        {
            bool canWalk = CanWalk_Implementation();
            if (onCanWalkValidated != null)
                onCanWalkValidated(ref canWalk);
            return canWalk;
        }

        protected virtual bool CanWalk_Implementation()
        {
            return false;
        }

        public bool CanCrouch()
        {
            bool canCrouch = CanCrouch_Implementation();
            if (onCanCrouchValidated != null)
                onCanCrouchValidated(ref canCrouch);
            return canCrouch;
        }

        protected virtual bool CanCrouch_Implementation()
        {
            return false;
        }

        public bool CanCrawl()
        {
            bool canCrawl = CanCrawl_Implementation();
            if (onCanCrawlValidated != null)
                onCanCrawlValidated(ref canCrawl);
            return canCrawl;
        }

        protected virtual bool CanCrawl_Implementation()
        {
            return false;
        }

        public bool CanJump()
        {
            bool canJump = CanJump_Implementation();
            if (onCanJumpValidated != null)
                onCanJumpValidated(ref canJump);
            return canJump;
        }

        protected virtual bool CanJump_Implementation()
        {
            return false;
        }

        public bool CanTurn()
        {
            bool canTurn = CanTurn_Implementation();
            if (onCanTurnValidated != null)
                onCanTurnValidated(ref canTurn);
            return canTurn;
        }

        protected virtual bool CanTurn_Implementation()
        {
            return false;
        }

        public void StopMove()
        {
            if (!ActiveMovement.IsNull())
                ActiveMovement.StopMove();
        }

        public void KeyMovement(Vector3 moveDirection, MovementState moveState)
        {
            if (!ActiveMovement.IsNull())
                ActiveMovement.KeyMovement(moveDirection, moveState);
        }

        public void PointClickMovement(Vector3 position)
        {
            if (!ActiveMovement.IsNull())
                ActiveMovement.PointClickMovement(position);
        }

        public void SetExtraMovementState(ExtraMovementState extraMovementState)
        {
            if (!ActiveMovement.IsNull())
                ActiveMovement.SetExtraMovementState(extraMovementState);
        }

        public void SetLookRotation(Quaternion rotation)
        {
            if (!ActiveMovement.IsNull())
                ActiveMovement.SetLookRotation(rotation);
        }

        public Quaternion GetLookRotation()
        {
            if (!ActiveMovement.IsNull())
                return ActiveMovement.GetLookRotation();
            return Quaternion.identity;
        }

        public void SetSmoothTurnSpeed(float speed)
        {
            if (!ActiveMovement.IsNull())
                ActiveMovement.SetSmoothTurnSpeed(speed);
        }

        public float GetSmoothTurnSpeed()
        {
            if (!ActiveMovement.IsNull())
                return ActiveMovement.GetSmoothTurnSpeed();
            return 0f;
        }

        public void SetShouldUseRootMotion(bool should)
        {
            if (!ActiveMovement.IsNull())
                ActiveMovement.SetShouldUseRootMotion(should);
        }

        public bool GetShouldUseRootMotion()
        {
            if (!ActiveMovement.IsNull())
                return ActiveMovement.GetShouldUseRootMotion();
            return false;
        }

        public void Teleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            if (ActiveMovement.IsNull())
            {
                // Can't teleport properly yet, try to teleport later
                _teleportingPosition = position;
                _teleportingRotation = rotation;
                _isTeleporting = true;
                _stillMoveAfterTeleport = stillMoveAfterTeleport;
                return;
            }
            if (FindGroundedPosition(position, GROUND_DETECTION_DISTANCE, out Vector3 groundedPosition))
            {
                // Set position to grounded position, to make it not float and fall
                position = groundedPosition;
            }
            if (IsServer)
            {
                // Teleport to the `position`, `rotation`
                ActiveMovement.Teleport(position, rotation, stillMoveAfterTeleport);
            }
            OnTeleport(position, rotation);
        }

        protected virtual void OnTeleport(Vector3 position, Quaternion rotation)
        {

        }

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = EntityTransform.position;
            if (!ActiveMovement.IsNull())
                return ActiveMovement.FindGroundedPosition(fromPosition, findDistance, out result);
            return true;
        }

        public void OnJumpForceApplied(float verticalVelocity)
        {
            if (onJumpForceApplied != null)
                onJumpForceApplied.Invoke(verticalVelocity);
        }
    }
}
