using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseAreaSkill : BaseSkill
    {
        [Category("Skill Casting")]
        public IncrementalFloat castDistance;

        [Category(2, "Area Settings")]
        public IncrementalFloat areaDuration;
        public IncrementalFloat applyDuration;
        public GameObject targetObjectPrefab;

        private GameObject cacheTargetObject;
        public GameObject CacheTargetObject
        {
            get
            {
                if (cacheTargetObject == null)
                {
                    cacheTargetObject = Instantiate(targetObjectPrefab);
                    cacheTargetObject.SetActive(false);
                }
                return cacheTargetObject;
            }
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
            int skillLevel = (int)data[0];
            if (BasePlayerCharacterController.Singleton is ShooterPlayerCharacterController)
                return AreaSkillControls.UpdateAimControls_Shooter(aimAxes, this, skillLevel, CacheTargetObject);
            return AreaSkillControls.UpdateAimControls(aimAxes, this, skillLevel, CacheTargetObject);
        }

        public override void FinishAimControls(bool isCancel)
        {
            if (CacheTargetObject != null)
                CacheTargetObject.SetActive(false);
        }
    }
}
