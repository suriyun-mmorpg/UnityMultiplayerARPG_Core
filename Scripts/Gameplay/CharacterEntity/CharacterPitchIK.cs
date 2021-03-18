using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterPitchIK : MonoBehaviour
    {
        public enum Axis
        {
            X, Y, Z
        }

        public Animator animator;
        public Axis axis = Axis.Z;
        public bool enableWhileStanding = true;
        public bool enableWhileCrouching = true;
        public bool enableWhileCrawling = true;
        public bool enableWhileSwiming = true;
        public HumanBodyBones pitchBone = HumanBodyBones.UpperChest;
        public Vector3 rotateOffset;
        public bool inversePitch = true;
        public float lerpDamping = 25f;
        [Range(0f, 180f)]
        public float maxAngle = 0f;
        private BaseCharacterEntity characterEntity;
        private float tempPitch;
        private Quaternion tempRotation;
        private Quaternion pitchRotation;

        public bool Enabling
        {
            get
            {
                if (!characterEntity || characterEntity.IsDead())
                    return false;
                if (characterEntity.MovementState == MovementState.IsUnderWater)
                {
                    if (!enableWhileSwiming)
                        return false;
                    return true;
                }
                switch (characterEntity.ExtraMovementState)
                {
                    case ExtraMovementState.IsCrouching:
                        if (!enableWhileCrouching)
                            return false;
                        break;
                    case ExtraMovementState.IsCrawling:
                        if (!enableWhileCrawling)
                            return false;
                        break;
                    default:
                        if (!enableWhileStanding)
                            return false;
                        break;
                }
                return true;
            }
        }

        private void Start()
        {
            characterEntity = GetComponentInParent<BaseCharacterEntity>();
            if (characterEntity == null)
            {
                enabled = false;
                return;
            }
            if (animator == null)
                animator = GetComponentInParent<Animator>();
            if (animator == null)
            {
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            if (!Enabling)
                return;
            tempPitch = characterEntity.Pitch;
            if (maxAngle > 0f)
            {
                if (tempPitch >= 180f && tempPitch < 360f - maxAngle)
                {
                    tempPitch = 360f - maxAngle;
                }
                else if (tempPitch < 180f && tempPitch > maxAngle)
                {
                    tempPitch = maxAngle;
                }
            }
            tempRotation = Quaternion.identity;
            switch (axis)
            {
                case Axis.X:
                    tempRotation = Quaternion.Euler(Vector3.left * tempPitch * (inversePitch ? -1 : 1));
                    break;
                case Axis.Y:
                    tempRotation = Quaternion.Euler(Vector3.up * tempPitch * (inversePitch ? -1 : 1));
                    break;
                case Axis.Z:
                    tempRotation = Quaternion.Euler(Vector3.forward * tempPitch * (inversePitch ? -1 : 1));
                    break;
            }
            tempRotation = tempRotation * Quaternion.Euler(rotateOffset);
            pitchRotation = Quaternion.Lerp(pitchRotation, tempRotation, lerpDamping * Time.deltaTime);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!Enabling)
                return;
            animator.SetBoneLocalRotation(pitchBone, pitchRotation);
        }
    }
}
