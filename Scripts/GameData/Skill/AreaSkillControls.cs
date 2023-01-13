using UnityEngine;

namespace MultiplayerARPG
{
    public class AreaSkillControls
    {
        public const float GROUND_DETECTION_DISTANCE = 30f;
        private static readonly RaycastHit[] findGroundRaycastHits = new RaycastHit[1000];

        public static bool IsMobile { get { return InputManager.useMobileInputOnNonMobile || Application.isMobilePlatform; } }

        public static AimPosition UpdateAimControls(Vector2 aimAxes, BaseAreaSkill skill, int skillLevel, GameObject targetObject)
        {
            if (IsMobile)
                return UpdateAimControls_Mobile(aimAxes, skill, skillLevel, targetObject);
            return UpdateAimControls_PC(Input.mousePosition, skill, skillLevel, targetObject);
        }

        public static AimPosition UpdateAimControls_Shooter(Vector2 aimAxes, BaseAreaSkill skill, int skillLevel, GameObject targetObject)
        {
            if (IsMobile)
                return UpdateAimControls_Mobile(aimAxes, skill, skillLevel, targetObject);
            return UpdateAimControls_PC(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f), skill, skillLevel, targetObject);
        }

        public static AimPosition UpdateAimControls_PC(Vector3 cursorPosition, BaseAreaSkill skill, int skillLevel, GameObject targetObject)
        {
            float castDistance = skill.castDistance.GetAmount(skillLevel);
            Vector3 position = GameplayUtils.CursorWorldPosition(Camera.main, cursorPosition);
            position = GameplayUtils.ClampPosition(GameInstance.PlayingCharacterEntity.EntityTransform.position, position, castDistance);
            position = PhysicUtils.FindGroundedPosition(position, findGroundRaycastHits, GROUND_DETECTION_DISTANCE, GameInstance.Singleton.GetAreaSkillGroundDetectionLayerMask());
            if (targetObject != null)
            {
                targetObject.SetActive(true);
                targetObject.transform.position = position;
            }
            return AimPosition.CreatePosition(position);
        }

        public static AimPosition UpdateAimControls_Mobile(Vector2 aimAxes, BaseAreaSkill skill, int skillLevel, GameObject targetObject)
        {
            float castDistance = skill.castDistance.GetAmount(skillLevel);
            Vector3 position = GameInstance.PlayingCharacterEntity.EntityTransform.position + (GameplayUtils.GetDirectionByAxes(Camera.main.transform, aimAxes.x, aimAxes.y) * castDistance);
            position = PhysicUtils.FindGroundedPosition(position, findGroundRaycastHits, GROUND_DETECTION_DISTANCE, GameInstance.Singleton.GetAreaSkillGroundDetectionLayerMask());
            if (targetObject != null)
            {
                targetObject.SetActive(true);
                targetObject.transform.position = position;
            }
            return AimPosition.CreatePosition(position);
        }
    }
}
