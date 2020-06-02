using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StandardAssets.Characters.Physics;

namespace MultiplayerARPG
{
    public class LegacyRigidBodyEntityMovementConversion : MonoBehaviour
    {
#if UNITY_EDITOR
        [ContextMenu("Convert From New Rigid Body Entity Movement")]
        public void ConvertFromNewRigidBodyEntityMovement()
        {
            float stoppingDistance = 0.1f;
            float jumpHeight = 2f;
            float backwardMoveSpeedRate = 0.75f;
            float underWaterThreshold = 0.75f;
            bool autoSwimToSurface = false;

            bool useRootMotionForMovement = false;
            bool useRootMotionForAirMovement = false;
            bool useRootMotionForJump = false;
            bool useRootMotionForFall = false;
            bool useRootMotionWhileNotMoving = false;
            RigidBodyEntityMovement newMovementSystem = GetComponent<RigidBodyEntityMovement>();
            if (newMovementSystem != null)
            {
                stoppingDistance = newMovementSystem.stoppingDistance;
                jumpHeight = newMovementSystem.jumpHeight;
                backwardMoveSpeedRate = newMovementSystem.backwardMoveSpeedRate;
                underWaterThreshold = newMovementSystem.underWaterThreshold;
                autoSwimToSurface = newMovementSystem.autoSwimToSurface;
                useRootMotionForMovement = newMovementSystem.useRootMotionForMovement;
                useRootMotionForAirMovement = newMovementSystem.useRootMotionForAirMovement;
                useRootMotionForJump = newMovementSystem.useRootMotionForJump;
                useRootMotionForFall = newMovementSystem.useRootMotionForFall;
                useRootMotionWhileNotMoving = newMovementSystem.useRootMotionWhileNotMoving;
                DestroyImmediate(newMovementSystem);
                Debug.Log("[LegacyRigidBodyEntityMovement] Removed `RigidBodyEntityMovement`");
            }
            OpenCharacterController controller = GetComponent<OpenCharacterController>();
            if (controller != null)
            {
                DestroyImmediate(controller);
                Debug.Log("[LegacyRigidBodyEntityMovement] Removed `OpenCharacterController`");
            }
            LegacyRigidBodyEntityMovement oldMovementSystem = GetComponent<LegacyRigidBodyEntityMovement>();
            if (oldMovementSystem == null)
                oldMovementSystem = gameObject.AddComponent<LegacyRigidBodyEntityMovement>();
            if (oldMovementSystem != null)
            {
                oldMovementSystem.stoppingDistance = stoppingDistance;
                oldMovementSystem.jumpHeight = jumpHeight;
                oldMovementSystem.backwardMoveSpeedRate = backwardMoveSpeedRate;
                oldMovementSystem.underWaterThreshold = underWaterThreshold;
                oldMovementSystem.autoSwimToSurface = autoSwimToSurface;
                oldMovementSystem.useRootMotionForMovement = useRootMotionForMovement;
                oldMovementSystem.useRootMotionForAirMovement = useRootMotionForAirMovement;
                oldMovementSystem.useRootMotionForJump = useRootMotionForJump;
                oldMovementSystem.useRootMotionForFall = useRootMotionForFall;
                oldMovementSystem.useRootMotionWhileNotMoving = useRootMotionWhileNotMoving;
            }
            Debug.Log("[LegacyRigidBodyEntityMovement] Converted");
        }
#endif
    }
}
