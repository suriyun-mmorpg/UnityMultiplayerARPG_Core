using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class EntityMovementFunctions
    {
        internal const int FIND_GROUND_HIT_ARRAY_LENGTH = 16;
        internal static readonly Vector3[] s_changePoseRaycastOffsets = new Vector3[]
        {
            Vector3.zero,
            new Vector3(0f, 0f, 1f),
            new Vector3(0f, 0f, -1f),
            new Vector3(1f, 0f, 0f),
            new Vector3(-1f, 0f, 0f),
            new Vector3(0.75f, 0f, 0.75f),
            new Vector3(0.75f, 0f, -0.75f),
            new Vector3(-0.75f, 0f, 0.75f),
            new Vector3(-0.75f, 0f, -0.75f),
        };
        internal static readonly List<Vector3> s_findGroundHitPoints = new List<Vector3>();

        public static ExtraMovementState ValidateExtraMovementState(this IEntityMovement movement, MovementState movementState, ExtraMovementState extraMovementState)
        {
            // Movement state can affect extra movement state
            if (movementState.Has(MovementState.IsUnderWater) ||
                movementState.Has(MovementState.IsClimbing))
            {
                // Extra movement states always none while under water
                extraMovementState = ExtraMovementState.None;
            }
            else if (!movement.Entity.CanMove() && extraMovementState == ExtraMovementState.IsSprinting)
            {
                // Character can't move, set extra movement state to none
                extraMovementState = ExtraMovementState.None;
            }
            else
            {
                switch (extraMovementState)
                {
                    case ExtraMovementState.IsSprinting:
                        if (!movementState.HasDirectionMovement())
                            extraMovementState = ExtraMovementState.None;
                        else if (!movement.Entity.CanSprint())
                            extraMovementState = ExtraMovementState.None;
                        else if (!movement.Entity.CanSideSprint && (movementState.Has(MovementState.Left) || movementState.Has(MovementState.Right)))
                            extraMovementState = ExtraMovementState.None;
                        else if (!movement.Entity.CanBackwardSprint && movementState.Has(MovementState.Backward))
                            extraMovementState = ExtraMovementState.None;
                        break;
                    case ExtraMovementState.IsWalking:
                        if (!movementState.HasDirectionMovement())
                            extraMovementState = ExtraMovementState.None;
                        else if (!movement.Entity.CanWalk())
                            extraMovementState = ExtraMovementState.None;
                        break;
                    case ExtraMovementState.IsCrouching:
                        if (!movement.Entity.CanCrouch())
                            extraMovementState = ExtraMovementState.None;
                        break;
                    case ExtraMovementState.IsCrawling:
                        if (!movement.Entity.CanCrawl())
                            extraMovementState = ExtraMovementState.None;
                        break;
                }
            }
            return extraMovementState;
        }

        public static bool AllowToChangePose(this IEntityMovement movement, float height, float radius, int layerMask)
        {
            Transform transform = movement.Entity.EntityTransform;
            for (int i = 0; i < s_changePoseRaycastOffsets.Length; ++i)
            {
                Vector3 origin = (s_changePoseRaycastOffsets[i] * radius * (transform.lossyScale.x + transform.lossyScale.z) * 0.5f) + transform.position;
                RaycastHit[] findGroundRaycastHits = ArrayPool<RaycastHit>.Shared.Rent(FIND_GROUND_HIT_ARRAY_LENGTH);
                int hitCount = Physics.RaycastNonAlloc(
                    origin, Vector3.up,
                    findGroundRaycastHits, height * transform.lossyScale.y,
                    layerMask, QueryTriggerInteraction.Ignore);
                ArrayPool<RaycastHit>.Shared.Return(findGroundRaycastHits);
#if UNITY_EDITOR
                Debug.DrawLine(origin, origin + Vector3.up * height * transform.lossyScale.y, Color.blue, 30f);
#endif
                if (hitCount > 0)
                    return false;
            }
            return true;
        }

        public static float[] CalculateCrawlRaycastDegrees(int crawlCheckRaycasts)
        {
            float[] result;
            if (crawlCheckRaycasts > 0)
            {
                result = new float[crawlCheckRaycasts];
                float increaseRaycastDegree = 360f / crawlCheckRaycasts;
                result = new float[crawlCheckRaycasts];
                for (int i = 0; i < crawlCheckRaycasts; ++i)
                {
                    result[i] = i * increaseRaycastDegree;
                }
                return result;
            }
            return null;
        }

        public static bool AllowToCrawl(this IEntityMovement movement, int crawlCheckRaycasts, float crawlCheckOffsets, float crawlCheckRadius)
        {
            if (crawlCheckRaycasts <= 0)
                return true;
            Transform transform = movement.Entity.EntityTransform;
            Vector3 center = new Vector3(transform.position.x, transform.position.y - crawlCheckOffsets, transform.position.z);
            float[] crawlRaycastDegrees = ArrayPool<float>.Shared.Rent(crawlCheckRaycasts);
            RaycastHit[] findGroundRaycastHits = ArrayPool<RaycastHit>.Shared.Rent(FIND_GROUND_HIT_ARRAY_LENGTH);
            crawlRaycastDegrees = CalculateCrawlRaycastDegrees(crawlCheckRaycasts);
            for (int i = 0; i < crawlCheckRaycasts; ++i)
            {
                int hitCount = Physics.RaycastNonAlloc(
                    center, Quaternion.Euler(0f, crawlRaycastDegrees[i], 0f) * transform.forward,
                    findGroundRaycastHits, crawlCheckRadius,
                    GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask(), QueryTriggerInteraction.Ignore);
                if (hitCount > 0)
                {
                    ArrayPool<float>.Shared.Return(crawlRaycastDegrees);
                    ArrayPool<RaycastHit>.Shared.Return(findGroundRaycastHits);
                    return false;
                }
            }
            ArrayPool<float>.Shared.Return(crawlRaycastDegrees);
            ArrayPool<RaycastHit>.Shared.Return(findGroundRaycastHits);
            return true;
        }

        public static Vector3 AdjustCrawlMotion(this IEntityMovement movement, MovementState movementState, ExtraMovementState extraMovementState, Vector3 motion, int crawlCheckRaycasts, float crawlCheckOffsets, float crawlCheckRadius, int layerMask)
        {
            if (extraMovementState != ExtraMovementState.IsCrawling || crawlCheckRaycasts == 0 || !movement.Entity.IsClient)
                return motion;
            Transform transform = movement.Entity.EntityTransform;
            Vector3 center = new Vector3(transform.position.x, transform.position.y - crawlCheckOffsets, transform.position.z);
            Vector3 moveDirection = motion.GetXZ().normalized;
            float nearestHitDistance = float.MaxValue;
            float nearestRaycastAngle = 0f;
            RaycastHit? nearestHit = null;
            float[] crawlRaycastDegrees = ArrayPool<float>.Shared.Rent(crawlCheckRaycasts);
            RaycastHit[] findGroundRaycastHits = ArrayPool<RaycastHit>.Shared.Rent(FIND_GROUND_HIT_ARRAY_LENGTH);
            crawlRaycastDegrees = CalculateCrawlRaycastDegrees(crawlCheckRaycasts);
            for (int i = 0; i < crawlCheckRaycasts; ++i)
            {
                Vector3 raycastDirection = Quaternion.Euler(0f, crawlRaycastDegrees[i], 0f) * moveDirection;
                float raycastAngle = Vector3.Angle(moveDirection, raycastDirection);
                if (raycastAngle > 90f)
                    continue;
                int hitCount = Physics.RaycastNonAlloc(center, raycastDirection, findGroundRaycastHits, crawlCheckRadius, layerMask, QueryTriggerInteraction.Ignore);
                if (hitCount <= 0)
                    continue;
                for (int j = 0; j < hitCount; ++j)
                {
                    RaycastHit hit = findGroundRaycastHits[j];
                    if (hit.distance >= nearestHitDistance)
                        continue;
                    nearestRaycastAngle = raycastAngle;
                    nearestHitDistance = hit.distance;
                    nearestHit = hit;
                }
            }
            ArrayPool<float>.Shared.Return(crawlRaycastDegrees);
            ArrayPool<RaycastHit>.Shared.Return(findGroundRaycastHits);
            if (nearestHit.HasValue)
            {
                Vector3 hitNormal = nearestHit.Value.normal;
                if (nearestHit.Value.distance < crawlCheckRadius * 0.9f)
                    return motion + hitNormal * crawlCheckRadius;
                return Vector3.ProjectOnPlane(motion, hitNormal);
            }
            return motion;
        }

        public static bool FindGroundedPosition(this IEntityMovement movement, Vector3 fromPosition, float radius, float findDistance, int layerMask, float resultUpOffsets, out Vector3 result)
        {
            s_findGroundHitPoints.Clear();
            Transform transform = movement.Entity.EntityTransform;
            RaycastHit[] findGroundRaycastHits = ArrayPool<RaycastHit>.Shared.Rent(FIND_GROUND_HIT_ARRAY_LENGTH);
            for (int i = 0; i < s_changePoseRaycastOffsets.Length; ++i)
            {
                Vector3 origin = (s_changePoseRaycastOffsets[i] * radius * (transform.lossyScale.x + transform.lossyScale.z) * 0.5f) + fromPosition + (Vector3.up * findDistance);
                int hitCount = Physics.RaycastNonAlloc(
                    origin, Vector3.down,
                    findGroundRaycastHits, findDistance + 0.5f,
                    layerMask, QueryTriggerInteraction.Ignore);
#if UNITY_EDITOR
                Debug.DrawLine(origin, origin + Vector3.down * (findDistance + 0.5f), Color.red, 30f);
#endif
                for (int j = 0; j < hitCount; ++j)
                {
                    s_findGroundHitPoints.Add(findGroundRaycastHits[j].point);
                }
            }
            ArrayPool<RaycastHit>.Shared.Return(findGroundRaycastHits);
            if (s_findGroundHitPoints.Count <= 0)
            {
                result = new Vector3(fromPosition.x, fromPosition.y + resultUpOffsets, fromPosition.z);
                return false;
            }
            float minDistance = float.MaxValue;
            float tempDistance;
            float resultY = fromPosition.y;
            for (int i = 0; i < s_findGroundHitPoints.Count; ++i)
            {
                tempDistance = Mathf.Abs(s_findGroundHitPoints[i].y - fromPosition.y);
                if (tempDistance < minDistance)
                {
                    minDistance = tempDistance;
                    resultY = s_findGroundHitPoints[i].y;
                }
            }
            result = new Vector3(fromPosition.x, resultY + resultUpOffsets, fromPosition.z);
            return true;
        }

#if UNITY_EDITOR
        public static void DrawCrawlCheckRaycasts(this IEntityMovement movement, int crawlCheckRaycasts, float crawlCheckOffsets, float crawlCheckRadius, Color crawlCheckGizmosColor)
        {
            if (crawlCheckRaycasts <= 0)
                return;
            Transform transform = movement.Entity.EntityTransform;
            Vector3 center = new Vector3(transform.position.x, transform.position.y - crawlCheckOffsets, transform.position.z);
            float[] crawlRaycastDegrees = ArrayPool<float>.Shared.Rent(crawlCheckRaycasts);
            crawlRaycastDegrees = CalculateCrawlRaycastDegrees(crawlCheckRaycasts);
            Gizmos.color = crawlCheckGizmosColor;
            for (int i = 0; i < crawlCheckRaycasts; ++i)
            {
                Gizmos.DrawLine(center, center + Quaternion.Euler(0f, crawlRaycastDegrees[i], 0f) * transform.forward * crawlCheckRadius);
            }
            ArrayPool<float>.Shared.Return(crawlRaycastDegrees);
        }
#endif
    }
}
