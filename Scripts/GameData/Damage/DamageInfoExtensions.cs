using UnityEngine;

namespace MultiplayerARPG
{
    public static class DamageInfoExtensions
    {
        public static void GetDamagePositionAndRotation(this IDamageInfo damageInfo, BaseCharacterEntity attacker, bool isLeftHand, bool forEffect, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            Transform damageTransform = forEffect ? damageInfo.GetDamageEffectTransform(attacker, isLeftHand) : damageInfo.GetDamageTransform(attacker, isLeftHand);
            position = damageTransform.position;
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension2D)
            {
                GetDamageRotation2D(attacker.Direction2D, out rotation);
                direction = attacker.Direction2D;
            }
            else
            {
                GetDamageRotation3D(position, aimPosition, stagger, out rotation);
                direction = rotation * Vector3.forward;
            }
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
