using UnityEngine;

namespace MultiplayerARPG
{
    public static class DamageInfoExtensions
    {
        public static void GetDamagePositionAndRotation(this IDamageInfo damageInfo, BaseCharacterEntity attacker, bool isLeftHand, AimPosition aimPosition, Vector3 spreadRange, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            Transform damageTransform = damageInfo.GetDamageTransform(attacker, isLeftHand);
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension2D)
            {
                position = damageTransform.position;
                direction = attacker.Direction2D;
                GetDamageRotation2D(direction, out rotation);
                return;
            }
            if (aimPosition.type == AimPositionType.Direction)
            {
                position = aimPosition.position;
                GetDamageRotation3D(position, damageTransform.forward, aimPosition.direction, spreadRange, out direction, out rotation);
            }
            else
            {
                // NOTE: Allow aim position type `None` here, may change it later
                position = damageTransform.position;
                GetDamageRotation3D(position, damageTransform.forward, aimPosition.position - position, spreadRange, out direction, out rotation);
            }
        }

        public static void GetDamageRotation2D(Vector2 aimDirection, out Quaternion rotation)
        {
            rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(aimDirection.y, aimDirection.x) * (180 / Mathf.PI)) + 90);
        }

        public static void GetDamageRotation3D(Vector3 position, Vector3 originForward, Vector3 aimDirection, Vector3 spreadRange, out Vector3 direction, out Quaternion rotation)
        {
            // Zero, sideways, or backward aim becomes forward.
            if (aimDirection.sqrMagnitude < 0.000001f ||
                Vector3.Dot(originForward, aimDirection) <= 0f)
            {
                aimDirection = originForward;
            }

            rotation =
                Quaternion.LookRotation(aimDirection.normalized, Vector3.up) *
                Quaternion.Euler(spreadRange);

            // Spread might push the final direction backward.
            direction = rotation * Vector3.forward;

            if (Vector3.Dot(originForward, direction) <= 0f)
            {
                rotation = Quaternion.LookRotation(
                    originForward,
                    Vector3.up);
            }
            direction = rotation * Vector3.forward;
        }
    }
}
