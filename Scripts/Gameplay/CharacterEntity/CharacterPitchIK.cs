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
        public HumanBodyBones pitchBone = HumanBodyBones.UpperChest;
        public Vector3 rotateOffset;
        [Range(-1f, 1f)]
        public float multiplier = -1f;
        public float lerpDamping = 25f;
        private BaseCharacterEntity characterEntity;
        private Quaternion tempRotation;
        private Quaternion pitchRotation;

        private void Start()
        {
            characterEntity = GetComponent<BaseCharacterEntity>();
            if (characterEntity == null)
            {
                enabled = false;
                return;
            }
            if (animator == null)
                animator = GetComponent<Animator>();
            if (animator == null)
            {
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            tempRotation = Quaternion.identity;
            switch (axis)
            {
                case Axis.X:
                    tempRotation = Quaternion.Euler(Vector3.left * characterEntity.Pitch * multiplier);
                    break;
                case Axis.Y:
                    tempRotation = Quaternion.Euler(Vector3.up * characterEntity.Pitch * multiplier);
                    break;
                case Axis.Z:
                    tempRotation = Quaternion.Euler(Vector3.forward * characterEntity.Pitch * multiplier);
                    break;
            }
            tempRotation = tempRotation * Quaternion.Euler(rotateOffset);
            pitchRotation = Quaternion.Lerp(pitchRotation, tempRotation, lerpDamping * Time.deltaTime);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            animator.SetBoneLocalRotation(pitchBone, pitchRotation);
        }
    }
}
