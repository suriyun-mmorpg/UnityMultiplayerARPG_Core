using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class AreaSkillControls
    {
        public const float GROUND_DETECTION_DISTANCE = 100f;
        
        public static bool IsMobile { get { return InputManager.useMobileInputOnNonMobile || Application.isMobilePlatform; } }

        public static Vector3? UpdateAimControls(Vector2 aimAxes, BaseAreaSkill skill, short skillLevel, GameObject targetObject)
        {
            if (IsMobile)
                return UpdateAimControls_Mobile(aimAxes, skill, skillLevel, targetObject);
            return UpdateAimControls_PC(Input.mousePosition, skill, skillLevel, targetObject);
        }

        public static Vector3? UpdateAimControls_Shooter(Vector2 aimAxes, BaseAreaSkill skill, short skillLevel, GameObject targetObject)
        {
            if (IsMobile)
                return UpdateAimControls_Mobile(aimAxes, skill, skillLevel, targetObject);
            return UpdateAimControls_PC(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f), skill, skillLevel, targetObject);
        }

        public static Vector3? UpdateAimControls_PC(Vector3 cursorPosition, BaseAreaSkill skill, short skillLevel, GameObject targetObject)
        {
            Vector3 position = CursorWorldPosition(cursorPosition);
            position = ValidateDistance(BasePlayerCharacterController.Singleton.PlayerCharacterEntity.CacheTransform.position, position, skill.castDistance.GetAmount(skillLevel));
            position = FindGround(position);
            if (targetObject != null)
            {
                targetObject.SetActive(true);
                targetObject.transform.position = position;
            }
            return position;
        }

        public static Vector3? UpdateAimControls_Mobile(Vector2 aimAxes, BaseAreaSkill skill, short skillLevel, GameObject targetObject)
        {
            float castDistance = skill.castDistance.GetAmount(skillLevel);
            Vector3 position = BasePlayerCharacterController.Singleton.PlayerCharacterEntity.CacheTransform.position + GetDirection(aimAxes.x, aimAxes.y) * castDistance;
            position = FindGround(position);
            if (targetObject != null)
            {
                targetObject.SetActive(true);
                targetObject.transform.position = position;
            }
            return position;
        }

        public static Vector3 GetDirection(float xAxis, float yAxis)
        {
            Vector3 aimDirection = Vector3.zero;
            switch (GameInstance.Singleton.DimensionType)
            {
                case DimensionType.Dimension3D:
                    Vector3 forward = Camera.main.transform.forward;
                    Vector3 right = Camera.main.transform.right;
                    forward.y = 0f;
                    right.y = 0f;
                    forward.Normalize();
                    right.Normalize();
                    aimDirection += forward * yAxis;
                    aimDirection += right * xAxis;
                    // normalize input if it exceeds 1 in combined length:
                    if (aimDirection.sqrMagnitude > 1)
                        aimDirection.Normalize();
                    break;
                case DimensionType.Dimension2D:
                    aimDirection = new Vector2(xAxis, yAxis);
                    break;
            }
            return aimDirection;
        }

        public static Vector3 CursorWorldPosition(Vector3 cursorPosition)
        {
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
            {
                RaycastHit tempHit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(cursorPosition), out tempHit))
                {
                    return tempHit.point;
                }
            }
            return Camera.main.ScreenToWorldPoint(cursorPosition);
        }

        public static Vector3 ValidateDistance(Vector3 centerPosition, Vector3 validatingPosition, float distance)
        {
            Vector3 offset = validatingPosition - centerPosition;
            return centerPosition + Vector3.ClampMagnitude(offset, distance);
        }

        public static Vector3 FindGround(Vector3 cursorPosition)
        {
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension2D)
                return cursorPosition;
            // Raycast to find hit floor
            Vector3? aboveHitPoint = null;
            Vector3? underHitPoint = null;
            int raycastLayerMask = GameInstance.Singleton.GetItemDropGroundDetectionLayerMask();
            RaycastHit tempHit;
            if (Physics.Raycast(cursorPosition, Vector3.up, out tempHit, GROUND_DETECTION_DISTANCE, raycastLayerMask))
                aboveHitPoint = tempHit.point;
            if (Physics.Raycast(cursorPosition, Vector3.down, out tempHit, GROUND_DETECTION_DISTANCE, raycastLayerMask))
                underHitPoint = tempHit.point;
            // Set drop position to nearest hit point
            if (aboveHitPoint.HasValue && underHitPoint.HasValue)
            {
                if (Vector3.Distance(cursorPosition, aboveHitPoint.Value) < Vector3.Distance(cursorPosition, underHitPoint.Value))
                    cursorPosition = aboveHitPoint.Value;
                else
                    cursorPosition = underHitPoint.Value;
            }
            else if (aboveHitPoint.HasValue)
                cursorPosition = aboveHitPoint.Value;
            else if (underHitPoint.HasValue)
                cursorPosition = underHitPoint.Value;
            return cursorPosition;
        }
    }
}
