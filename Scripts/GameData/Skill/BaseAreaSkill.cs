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
    }
}
