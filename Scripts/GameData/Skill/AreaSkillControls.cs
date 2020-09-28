using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class AreaSkillControls
    {
        public const float GROUND_DETECTION_DISTANCE = 100f;
        public const int GROUND_DETECTION_RAYCAST_LENGTH = 64;

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
            float castDistance = skill.castDistance.GetAmount(skillLevel);
            Vector3 position = GameplayUtils.CursorWorldPosition(Camera.main, cursorPosition);
            position = GameplayUtils.ClampPosition(BasePlayerCharacterController.OwningCharacter.CacheTransform.position, position, castDistance);
            position = PhysicUtils.FindGroundedPosition(position, GROUND_DETECTION_RAYCAST_LENGTH, GROUND_DETECTION_DISTANCE, GameInstance.Singleton.GetAreaSkillGroundDetectionLayerMask());
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
            Vector3 position = BasePlayerCharacterController.OwningCharacter.CacheTransform.position + (GameplayUtils.GetDirectionByAxes(Camera.main.transform, aimAxes.x, aimAxes.y) * castDistance);
            position = PhysicUtils.FindGroundedPosition(position, GROUND_DETECTION_RAYCAST_LENGTH, GROUND_DETECTION_DISTANCE, GameInstance.Singleton.GetAreaSkillGroundDetectionLayerMask());
            if (targetObject != null)
            {
                targetObject.SetActive(true);
                targetObject.transform.position = position;
            }
            return position;
        }
    }
}
