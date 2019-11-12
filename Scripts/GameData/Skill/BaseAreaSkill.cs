using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseAreaSkill : BaseSkill
    {
        public IncrementalFloat castDistance;
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

        public override SkillType GetSkillType()
        {
            return SkillType.Active;
        }

        public override float GetCastDistance(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand)
        {
            return castDistance.GetAmount(skillLevel);
        }

        public override float GetCastFov(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand)
        {
            return 360f;
        }

        public override bool HasCustomAimControls()
        {
            return true;
        }

        public override Vector3? UpdateAimControls(Vector2 aimAxes, short skillLevel)
        {
            if (BasePlayerCharacterController.Singleton is PlayerCharacterController)
                return AreaSkillControls.UpdateAimControls(aimAxes, this, skillLevel, CacheTargetObject);
            if (BasePlayerCharacterController.Singleton is ShooterPlayerCharacterController)
                return AreaSkillControls.UpdateAimControls_Shooter(aimAxes, this, skillLevel, CacheTargetObject);
            return BasePlayerCharacterController.OwningCharacter.CacheTransform.position;
        }

        public override void FinishAimControls()
        {
            if (CacheTargetObject != null)
                CacheTargetObject.SetActive(false);
        }
    }
}
