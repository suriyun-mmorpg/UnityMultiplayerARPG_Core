using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public abstract class BaseEntityMovement : BaseGameEntityComponent<BaseGameEntity>, IEntityMovement
    {
        public BaseGameEntity Entity { get { return CacheEntity; } }
        public abstract float StoppingDistance { get; }
        public abstract void KeyMovement(Vector3 moveDirection, MovementState movementState);
        public abstract void PointClickMovement(Vector3 position);
        public abstract void StopMove();
        public abstract void SetLookRotation(Quaternion rotation);
        public abstract Quaternion GetLookRotation();
        public abstract void Teleport(Vector3 position);
        public abstract bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result);

        public virtual void PrepareRelatesData()
        {
            // Do nothing
        }
    }
}
