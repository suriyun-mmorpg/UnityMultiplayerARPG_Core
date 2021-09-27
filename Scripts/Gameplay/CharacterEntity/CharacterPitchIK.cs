using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterPitchIK : MonoBehaviour
    {
        public enum Axis
        {
            X, Y, Z
        }

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
        private float tempPitch;
        private Quaternion tempRotation;
        public Quaternion PitchRotation { get; private set; }
        public BaseCharacterEntity CharacterEntity { get; private set; }
        public CharacterPitchIKManager Manager { get; private set; }

        public bool Enabling
        {
            get
            {
                if (!enabled)
                    return false;
                if (!CharacterEntity || CharacterEntity.IsDead())
                    return false;
                if (CharacterEntity.MovementState == MovementState.IsUnderWater)
                {
                    if (!enableWhileSwiming)
                        return false;
                    return true;
                }
                switch (CharacterEntity.ExtraMovementState)
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

        private void Awake()
        {
            CharacterEntity = GetComponentInParent<BaseCharacterEntity>();
            if (CharacterEntity == null)
            {
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            Manager = CharacterEntity.gameObject.GetOrAddComponent<CharacterPitchIKManager>();
            Manager.Register(this);
        }

        private void OnDestroy()
        {
            if (Manager != null)
                Manager.Unregister(this);
        }

        public void UpdatePitchRotation()
        {
            if (!Enabling)
                return;
            // Clamp pitch
            tempPitch = CharacterEntity.Pitch;
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
            // Find pitch rotation
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
            if (lerpDamping > 0f)
            {
                PitchRotation = Quaternion.Lerp(PitchRotation, tempRotation, lerpDamping * Time.deltaTime);
            }
            else
            {
                PitchRotation = tempRotation;
            }
        }
    }
}
