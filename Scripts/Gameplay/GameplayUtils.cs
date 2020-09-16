using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class GameplayUtils
    {
        public static Vector3 CursorWorldPosition(Camera camera, Vector3 cursorPosition, float distance = 100f)
        {
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
            {
                RaycastHit tempHit;
                if (Physics.Raycast(camera.ScreenPointToRay(cursorPosition), out tempHit, distance, GameInstance.Singleton.GetTargetLayerMask()))
                {
                    return tempHit.point;
                }
            }
            return camera.ScreenToWorldPoint(cursorPosition);
        }

        public static Vector3 ClampPosition(Vector3 centerPosition, Vector3 validatingPosition, float distance)
        {
            Vector3 offset = validatingPosition - centerPosition;
            return centerPosition + Vector3.ClampMagnitude(offset, distance);
        }

        public static Vector3 GetDirectionByAxes(Transform cameraTransform, float xAxis, float yAxis)
        {
            Vector3 aimDirection = Vector3.zero;
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
            {
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();
                aimDirection += forward * yAxis;
                aimDirection += right * xAxis;
                // normalize input if it exceeds 1 in combined length:
                if (aimDirection.sqrMagnitude > 1)
                    aimDirection.Normalize();
                return aimDirection;
            }
            else
            {
                return new Vector2(xAxis, yAxis);
            }
        }

        public static DirectionType2D GetDirectionTypeByVector2(Vector2 direction)
        {
            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);
            if (absX / absY > 0.8f)
            {
                if (direction.x < 0) return DirectionType2D.Left;
                if (direction.x > 0) return DirectionType2D.Right;
            }
            else if (absY / absX > 0.8f)
            {
                if (direction.y < 0) return DirectionType2D.Down;
                if (direction.y > 0) return DirectionType2D.Up;
            }
            else
            {
                DirectionType2D result = DirectionType2D.Down;
                if (direction.x > 0.01f)
                {
                    result = DirectionType2D.Left;
                    if (direction.y > 0.01f)
                        result |= DirectionType2D.Up;
                    if (direction.y < -0.01f)
                        result |= DirectionType2D.Down;
                }
                else if (direction.x < -0.01f)
                {
                    result = DirectionType2D.Right;
                    if (direction.y > 0.01f)
                        result |= DirectionType2D.Up;
                    if (direction.y < -0.01f)
                        result |= DirectionType2D.Down;
                }
                else if (direction.y > 0.01f)
                {
                    result = DirectionType2D.Up;
                }
                return result;
            }
            return DirectionType2D.Down;
        }

        public static Bounds MakeLocalBoundsByCollider(Transform transform)
        {
            Bounds result = new Bounds();
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
            {
                Collider col = transform.GetComponent<Collider>();
                if (col != null)
                {
                    result = col.bounds;
                    result.center = result.center - transform.position;
                }
            }
            else
            {
                Collider2D col2d = transform.GetComponent<Collider2D>();
                if (col2d != null)
                {
                    result = col2d.bounds;
                    result.center = result.center - transform.position;
                }
            }
            return result;
        }

        public static float BoundsDistance(Bounds a, Bounds b)
        {
            return Vector3.Distance(a.ClosestPoint(b.center), b.ClosestPoint(a.center));
        }

        public static float BoundsDistance(Bounds a, Vector3 b)
        {
            return Vector3.Distance(a.ClosestPoint(b), b);
        }
    }
}
