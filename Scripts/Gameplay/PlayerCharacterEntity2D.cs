using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PlayerCharacterEntity2D : BasePlayerCharacterEntity
    {
        #region Settings
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        #endregion

        #region Temp data
        protected Collider2D[] overlapColliders2D = new Collider2D[OVERLAP_COLLIDER_SIZE];
        #endregion

        public override float StoppingDistance
        {
            get { return stoppingDistance; }
        }

        public override bool IsMoving()
        {
            throw new System.NotImplementedException();
        }

        public override void KeyMovement(Vector3 direction, bool isJump)
        {
            throw new System.NotImplementedException();
        }

        public override void PointClickMovement(Vector3 position)
        {
            throw new System.NotImplementedException();
        }

        public override void StopMove()
        {
            throw new System.NotImplementedException();
        }

        protected override int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D, layerMask);
        }

        protected override GameObject GetOverlapObject(int index)
        {
            return tempGameObject = overlapColliders2D[index].gameObject;
        }

        protected override bool IsPositionInFov(float fov, Vector3 position)
        {
            var halfFov = fov * 0.5f;
            var angle = Vector2.Angle((CacheTransform.position - position).normalized, CacheTransform.up);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return (angle < 180 + halfFov && angle > 180 - halfFov);
        }
    }
}
