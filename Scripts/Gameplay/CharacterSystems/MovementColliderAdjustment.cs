using UnityEngine;
using StandardAssets.Characters.Physics;

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
            public bool applyToComponent;
#endif
        }

        [SerializeField]
        private Settings standSettings = new Settings()
        {
#if UNITY_EDITOR
            gizmosColor = Color.blue
#endif
        };
        [SerializeField]
        private Settings crouchSettings = new Settings()
        {
#if UNITY_EDITOR
            gizmosColor = Color.magenta
#endif
        };
        [SerializeField]
        private Settings crawlSettings = new Settings()
        {
#if UNITY_EDITOR
            gizmosColor = Color.red
#endif
        };
        [SerializeField]
        private Settings swimSettings = new Settings()
        {
#if UNITY_EDITOR
            gizmosColor = Color.yellow
#endif
        };

        private OpenCharacterController openCharacterController;
        private CapsuleCollider capsuleCollider;
        private bool previousIsUnderWater;
        private ExtraMovementState previousExtraMovementState;

        public override void EntityAwake()
        {
            openCharacterController = GetComponent<OpenCharacterController>();
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            openCharacterController = GetComponent<OpenCharacterController>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            if (openCharacterController != null)
            {
                Settings tempSettings;
                bool anyDirectionNotY = false;
                if (standSettings.direction != Direction.Y)
                {
                    anyDirectionNotY = true;
                    tempSettings = standSettings;
                    tempSettings.direction = Direction.Y;
                    standSettings = tempSettings;
                }
                if (crouchSettings.direction != Direction.Y)
                {
                    anyDirectionNotY = true;
                    tempSettings = crouchSettings;
                    tempSettings.direction = Direction.Y;
                    crouchSettings = tempSettings;
                }
                if (crawlSettings.direction != Direction.Y)
                {
                    anyDirectionNotY = true;
                    tempSettings = crawlSettings;
                    tempSettings.direction = Direction.Y;
                    crawlSettings = tempSettings;
                }
                if (swimSettings.direction != Direction.Y)
                {
                    anyDirectionNotY = true;
                    tempSettings = swimSettings;
                    tempSettings.direction = Direction.Y;
                    swimSettings = tempSettings;
                }
                if (anyDirectionNotY)
                    Debug.LogWarning("Direction for `OpenCharacterController` can set to `Y` only");
            }
            ApplyingSettings(ref standSettings);
            ApplyingSettings(ref crouchSettings);
            ApplyingSettings(ref crawlSettings);
            ApplyingSettings(ref swimSettings);
        }

        private void ApplyingSettings(ref Settings settings)
        {
            if (settings.applyToComponent)
            {
                Apply(settings);
                settings.applyToComponent = false;
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
            if (openCharacterController == null && capsuleCollider == null)
                return;

            bool isUnderWater = Entity.MovementState.Has(MovementState.IsUnderWater);
            if (isUnderWater && isUnderWater != previousIsUnderWater)
            {
                Apply(swimSettings);
            }
            else if (Entity.ExtraMovementState != previousExtraMovementState)
            {
                switch (Entity.ExtraMovementState)
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
            previousIsUnderWater = isUnderWater;
            previousExtraMovementState = Entity.ExtraMovementState;
        }

        private void Apply(Settings settings)
        {
            if (openCharacterController != null)
            {
                openCharacterController.SetRadiusHeightAndCenter(settings.radius, settings.height, settings.center, true, true);
            }
            else if (capsuleCollider != null)
            {
                capsuleCollider.center = settings.center;
                capsuleCollider.radius = settings.radius;
                capsuleCollider.height = settings.height;
                capsuleCollider.direction = (int)settings.direction;
            }
        }
    }
}