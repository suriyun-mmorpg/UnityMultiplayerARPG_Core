using UnityEngine;
using System.Collections;

namespace MultiplayerARPG
{
    public class MovementColliderAdjustment : BaseGameEntityComponent<BaseGameEntity>
    {
        [System.Serializable]
        public struct Settings
        {
            public Vector3 center;
            public float radius;
            public float height;
        }
        
        [SerializeField]
        private Settings standSettings;
        [SerializeField]
        private Settings crouchSettings;
        [SerializeField]
        private Settings crawlSettings;
        [SerializeField]
        private Settings swimSettings;

        private CapsuleCollider capsuleCollider;

        public override void EntityAwake()
        {
            if (capsuleCollider == null)
                capsuleCollider = GetComponent<CapsuleCollider>();
        }

        public override void EntityLateUpdate()
        {
            if (capsuleCollider == null)
                return;

            if (CacheEntity.IsUnderWater)
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
        }
    }
}