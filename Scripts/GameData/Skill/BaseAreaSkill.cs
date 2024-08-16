using Insthync.AddressableAssetTools;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MultiplayerARPG
{
    public abstract class BaseAreaSkill : BaseSkill
    {
        [Category("Skill Casting")]
        public IncrementalFloat castDistance;

        [Category(2, "Area Settings")]
        public IncrementalFloat areaDuration;
        public IncrementalFloat applyDuration;
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [SerializeField]
        [AddressableAssetConversion(nameof(addressableTargetObjectPrefab))]
        private GameObject targetObjectPrefab;
#endif
        public GameObject TargetObjectPrefab
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return targetObjectPrefab;
#else
                return null;
#endif
            }
        }

        [SerializeField]
        private AssetReferenceGameObject addressableTargetObjectPrefab;
        public AssetReferenceGameObject AddressableTargetObjectPrefab
        {
            get { return addressableTargetObjectPrefab; }
        }

        public override SkillType SkillType
        {
            get { return SkillType.Active; }
        }

        public override float GetCastDistance(BaseCharacterEntity skillUser, int skillLevel, bool isLeftHand)
        {
            return castDistance.GetAmount(skillLevel);
        }

        public override float GetCastFov(BaseCharacterEntity skillUser, int skillLevel, bool isLeftHand)
        {
            return 360f;
        }

        public override bool HasCustomAimControls()
        {
            return true;
        }

        public override AimPosition UpdateAimControls(Vector2 aimAxes, params object[] data)
        {
            return BasePlayerCharacterController.Singleton.AreaSkillAimController.UpdateAimControls(aimAxes, this, (int)data[0]);
        }

        public override void FinishAimControls(bool isCancel)
        {
            BasePlayerCharacterController.Singleton.AreaSkillAimController.FinishAimControls(isCancel);
        }

        public override Vector3 GetDefaultAttackAimPosition(BaseCharacterEntity skillUser, int skillLevel, bool isLeftHand, IDamageableEntity target)
        {
            return target.Entity.MovementTransform.position;
        }
    }
}
