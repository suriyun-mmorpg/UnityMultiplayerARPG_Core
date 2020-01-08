using UnityEngine;
using System.Collections;

namespace MultiplayerARPG
{
    public class MovementColliderAdjustment : BaseGameEntityComponent<BaseGameEntity>
    {
        public enum Direction : int
        {
            X = 0,
            Y = 1,
            Z = 2,
        }

        [System.Serializable]
        public struct Settings
        {
            public Vector3 center;
            public float radius;
            public float height;
            public Direction direction;
#if UNITY_EDITOR
            public bool drawGizmos;
            public Color gizmosColor;
            [Header("Editor Tools")]
            public bool applyToCapsule;
#endif
        }

        [SerializeField]
        private Settings standSettings = new Settings()
        {
            gizmosColor = Color.blue
        };
        [SerializeField]
        private Settings crouchSettings = new Settings()
        {
            gizmosColor = Color.magenta
        };
        [SerializeField]
        private Settings crawlSettings = new Settings()
        {
            gizmosColor = Color.red
        };
        [SerializeField]
        private Settings swimSettings = new Settings()
        {
            gizmosColor = Color.yellow
        };

        private CapsuleCollider capsuleCollider;

        public override void EntityAwake()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            ApplyingSettings(ref standSettings);
            ApplyingSettings(ref crouchSettings);
            ApplyingSettings(ref crawlSettings);
            ApplyingSettings(ref swimSettings);
        }

        private void ApplyingSettings(ref Settings settings)
        {
            if (settings.applyToCapsule)
            {
                Apply(settings);
                settings.applyToCapsule = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmos(standSettings);
            DrawGizmos(crouchSettings);
            DrawGizmos(crawlSettings);
            DrawGizmos(swimSettings);
        }

        private void DrawGizmos(Settings settings)
        {
            if (!settings.drawGizmos)
                return;
            Gizmos.color = settings.gizmosColor;
            float horizontalScale = transform.localScale.x > transform.localScale.z ? transform.localScale.x : transform.localScale.z;
            float verticalScale = transform.localScale.y;
            Vector3 localPosition = transform.localPosition;
            Vector3 center = settings.center * verticalScale;
            float height = (settings.height - settings.radius * 2) / 2 * verticalScale;
            float radius = settings.radius * horizontalScale;
            switch (settings.direction)
            {
                case Direction.X:
                    Gizmos.DrawWireSphere(localPosition + center + Vector3.right * height, radius);
                    Gizmos.DrawWireSphere(localPosition + center + Vector3.left * height, radius);
                    Gizmos.DrawLine(localPosition + center + (Vector3.up * radius) + Vector3.left * height,
                        localPosition + center + (Vector3.up * radius) + Vector3.right * height);
                    Gizmos.DrawLine(localPosition + center + (Vector3.down * radius) + Vector3.left * height,
                        localPosition + center + (Vector3.down * radius) + Vector3.right * height);
                    Gizmos.DrawLine(localPosition + center + (Vector3.forward * radius) + Vector3.left * height,
                        localPosition + center + (Vector3.forward * radius) + Vector3.right * height);
                    Gizmos.DrawLine(localPosition + center + (Vector3.back * radius) + Vector3.left * height,
                        localPosition + center + (Vector3.back * radius) + Vector3.right * height);
                    break;
                case Direction.Y:
                    Gizmos.DrawWireSphere(localPosition + center + Vector3.up * height, radius);
                    Gizmos.DrawWireSphere(localPosition + center + Vector3.down * height, radius);
                    Gizmos.DrawLine(localPosition + center + (Vector3.forward * radius) + Vector3.down * height,
                        localPosition + center + (Vector3.forward * radius) + Vector3.up * height);
                    Gizmos.DrawLine(localPosition + center + (Vector3.back * radius) + Vector3.down * height,
                        localPosition + center + (Vector3.back * radius) + Vector3.up * height);
                    Gizmos.DrawLine(localPosition + center + (Vector3.right * radius) + Vector3.down * height,
                        localPosition + center + (Vector3.right * radius) + Vector3.up * height);
                    Gizmos.DrawLine(localPosition + center + (Vector3.left * radius) + Vector3.down * height,
                        localPosition + center + (Vector3.left * radius) + Vector3.up * height);
                    break;
                case Direction.Z:
                    Gizmos.DrawWireSphere(localPosition + center + Vector3.forward * height, radius);
                    Gizmos.DrawWireSphere(localPosition + center + Vector3.back * height, radius);
                    Gizmos.DrawLine(localPosition + center + (Vector3.up * radius) + Vector3.back * height,
                        localPosition + center + (Vector3.up * radius) + Vector3.forward * height);
                    Gizmos.DrawLine(localPosition + center + (Vector3.down * radius) + Vector3.back * height,
                        localPosition + center + (Vector3.down * radius) + Vector3.forward * height);
                    Gizmos.DrawLine(localPosition + center + (Vector3.forward * radius) + Vector3.back * height,
                        localPosition + center + (Vector3.right * radius) + Vector3.forward * height);
                    Gizmos.DrawLine(localPosition + center + (Vector3.back * radius) + Vector3.back * height,
                        localPosition + center + (Vector3.left * radius) + Vector3.forward * height);
                    break;
            }
        }
#endif

        public override void EntityLateUpdate()
        {
            if (capsuleCollider == null)
                return;

            if (CacheEntity.MovementState.HasFlag(MovementState.IsUnderWater))
            {
                Apply(swimSettings);
            }
            else
            {
                switch (CacheEntity.ExtraMovementState)
                {
                    case ExtraMovementState.IsCrouching:
                        Apply(crouchSettings);
                        break;
                    case ExtraMovementState.IsCrawling:
                        Apply(crawlSettings);
                        break;
                    default:
                        Apply(standSettings);
                        break;
                }
            }
        }

        private void Apply(Settings settings)
        {
            capsuleCollider.center = settings.center;
            capsuleCollider.radius = settings.radius;
            capsuleCollider.height = settings.height;
            capsuleCollider.direction = (int)settings.direction;
        }
    }
}