using UnityEngine;

namespace MultiplayerARPG
{
    public static class DamageInfoExtensions
    {
        public static void GetDamagePositionAndRotation(this IDamageInfo damageInfo, BaseCharacterEntity attacker, bool isLeftHand, AimPosition aimPosition, Vector3 stagger, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension2D)
            {
                Transform damageTransform = damageInfo.GetDamageTransform(attacker, isLeftHand);
                position = damageTransform.position;
                GetDamageRotation2D(attacker.Direction2D, out rotation);
                direction = attacker.Direction2D;
#if UNITY_EDITOR
                attacker.SetDebugDamage(position, direction, rotation, isLeftHand);
#endif
                return;
            }
            if (aimPosition.type == AimPositionType.Direction)
            {
                position = aimPosition.position;
                rotation = Quaternion.Euler(Quaternion.LookRotation(aimPosition.direction).eulerAngles + stagger);
                direction = rotation * Vector3.forward;
            }
            else
            {
                // NOTE: Allow aim position type `None` here, may change it later
                Transform damageTransform = damageInfo.GetDamageTransform(attacker, isLeftHand);
                position = damageTransform.position;
                GetDamageRotation3D(position, aimPosition.position, stagger, out rotation);
                direction = rotation * Vector3.forward;
            }
#if UNITY_EDITOR
            attacker.SetDebugDamage(position, direction, rotation, isLeftHand);
#endif
        }

        public static void GetDamageRotation2D(Vector2 aimDirection, out Quaternion rotation)
        {
            rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(aimDirection.y, aimDirection.x) * (180 / Mathf.PI)) + 90);
        }

        public static void GetDamageRotation3D(Vector3 damagePosition, Vector3 aimPosition, Vector3 stagger, out Quaternion rotation)
        {
            rotation = Quaternion.Euler(Quaternion.LookRotation(aimPosition - damagePosition).eulerAngles + stagger);
        }
    }
}
