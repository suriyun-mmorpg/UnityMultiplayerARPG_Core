using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class AnimatorCharacterModel2D : AnimatorCharacterModel, ICharacterModel2D
    {
        // Animator variables
        public static readonly int ANIM_DIRECTION_X = Animator.StringToHash("DirectionX");
        public static readonly int ANIM_DIRECTION_Y = Animator.StringToHash("DirectionY");

        public DirectionType CurrentDirectionType { get; set; }

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            bool hasChanges = false;
            RuntimeAnimatorController changingAnimatorController;
            switch (controllerType)
            {
                case AnimatorControllerType.Simple:
                    changingAnimatorController = Resources.Load("__Animator/__4DirCharacter2D") as RuntimeAnimatorController;
                    if (changingAnimatorController != null &&
                        changingAnimatorController != animatorController)
                    {
                        animatorController = changingAnimatorController;
                        hasChanges = true;
                    }
                    if (actionStateLayer != 0)
                    {
                        actionStateLayer = 0;
                        hasChanges = true;
                    }
                    if (castSkillStateLayer != 0)
                    {
                        castSkillStateLayer = 0;
                        hasChanges = true;
                    }
                    break;
                case AnimatorControllerType.Advance:
                    changingAnimatorController = Resources.Load("__Animator/__8DirCharacter2D") as RuntimeAnimatorController;
                    if (changingAnimatorController != null &&
                        changingAnimatorController != animatorController)
                    {
                        animatorController = changingAnimatorController;
                        hasChanges = true;
                    }
                    if (actionStateLayer != 0)
                    {
                        actionStateLayer = 0;
                        hasChanges = true;
                    }
                    if (castSkillStateLayer != 0)
                    {
                        castSkillStateLayer = 0;
                        hasChanges = true;
                    }
                    break;
            }
            if (hasChanges)
                EditorUtility.SetDirty(this);
#endif
        }

        public override void UpdateAnimation(bool isDead, MovementFlag movementState, float playMoveSpeedMultiplier = 1)
        {
            if (!animator.gameObject.activeInHierarchy)
                return;

            // Left/Right
            if (CurrentDirectionType.HasFlag(DirectionType.Left))
                animator.SetFloat(ANIM_DIRECTION_X, -1);
            else if (CurrentDirectionType.HasFlag(DirectionType.Right))
                animator.SetFloat(ANIM_DIRECTION_X, 1);

            // Up/Down
            if (CurrentDirectionType.HasFlag(DirectionType.Down))
                animator.SetFloat(ANIM_DIRECTION_Y, -1);
            else if (CurrentDirectionType.HasFlag(DirectionType.Up))
                animator.SetFloat(ANIM_DIRECTION_Y, 1);

            base.UpdateAnimation(isDead, movementState, playMoveSpeedMultiplier);
        }
    }
}
