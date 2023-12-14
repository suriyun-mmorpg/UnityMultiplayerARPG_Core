using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        protected bool _lastGrounded;
        protected Vector3 _lastGroundedPosition;

        public override bool ShouldUseRootMotion
        {
            get
            {
                return (IsAttacking && IsUseRootMotionWhileAttacking) ||
                    (IsUsingSkill && IsUseRootMotionWhileUsingSkill) ||
                    (IsReloading && IsUseRootMotionWhileReloading) ||
                    (IsCharging && IsUseRootMotionWhileCharging);
            }
        }

        public override bool SkipMovementValidation
        {
            get
            {
                return (IsAttacking && IsSkipMovementValidationWhileAttacking) ||
                    (IsUsingSkill && IsSkipMovementValidationWhileUsingSkill) ||
                    (IsReloading && IsSkipMovementValidationWhileReloading) ||
                    (IsCharging && IsSkipMovementValidationWhileCharging);
            }
        }

        public override float GetMoveSpeed(MovementState movementState, ExtraMovementState extraMovementState)
        {
            float moveSpeed = CachedData.MoveSpeed;
            float time = Time.unscaledTime;
            if (IsAttacking || time - LastAttackEndTime < CurrentGameInstance.returnMoveSpeedDelayAfterAction)
            {
                moveSpeed *= MoveSpeedRateWhileAttacking;
            }
            else if (IsUsingSkill || time - LastUseSkillEndTime < CurrentGameInstance.returnMoveSpeedDelayAfterAction)
            {
                moveSpeed *= MoveSpeedRateWhileUsingSkill;
            }
            else if (IsReloading)
            {
                moveSpeed *= MoveSpeedRateWhileReloading;
            }
            else if (IsCharging)
            {
                moveSpeed *= MoveSpeedRateWhileCharging;
            }
            if (movementState.Has(MovementState.IsUnderWater))
            {
                moveSpeed *= CurrentGameplayRule.GetSwimMoveSpeedRate(this);
            }
            else
            {
                switch (extraMovementState)
                {
                    case ExtraMovementState.IsSprinting:
                        moveSpeed *= CurrentGameplayRule.GetSprintMoveSpeedRate(this);
                        break;
                    case ExtraMovementState.IsWalking:
                        moveSpeed *= CurrentGameplayRule.GetWalkMoveSpeedRate(this);
                        break;
                    case ExtraMovementState.IsCrouching:
                        moveSpeed *= CurrentGameplayRule.GetCrouchMoveSpeedRate(this);
                        break;
                    case ExtraMovementState.IsCrawling:
                        moveSpeed *= CurrentGameplayRule.GetCrawlMoveSpeedRate(this);
                        break;
                }
            }

            if (CachedData.IsOverweight)
                moveSpeed *= CurrentGameplayRule.GetOverweightMoveSpeedRate(this);

            return moveSpeed;
        }

        public override float GetJumpHeight(MovementState movementState, ExtraMovementState extraMovementState)
        {
            return CachedData.JumpHeight;
        }

        public override float GetGravityRate(MovementState movementState, ExtraMovementState extraMovementState)
        {
            return 1f + CachedData.GravityRate;
        }

        protected override bool CanMove_Implementation()
        {
            if (this.IsDead())
                return false;
            if (CachedData.DisallowMove)
                return false;
            return true;
        }

        protected override bool CanSprint_Implementation()
        {
            if (!MovementState.Has(MovementState.IsGrounded) || MovementState.Has(MovementState.IsUnderWater))
                return false;
            if (CachedData.DisallowSprint)
                return false;
            return CurrentStamina > 0;
        }

        protected override bool CanWalk_Implementation()
        {
            if (!MovementState.Has(MovementState.IsGrounded) || MovementState.Has(MovementState.IsUnderWater))
                return false;
            if (CachedData.DisallowWalk)
                return false;
            return true;
        }

        protected override bool CanCrouch_Implementation()
        {
            if (!MovementState.Has(MovementState.IsGrounded) || MovementState.Has(MovementState.IsUnderWater))
                return false;
            if (CachedData.DisallowCrouch)
                return false;
            return true;
        }

        protected override bool CanCrawl_Implementation()
        {
            if (!MovementState.Has(MovementState.IsGrounded) || MovementState.Has(MovementState.IsUnderWater))
                return false;
            if (CachedData.DisallowCrawl)
                return false;
            return true;
        }

        protected override bool CanJump_Implementation()
        {
            if (CachedData.DisallowJump)
            {
                return false;
            }
            if (IsAttacking && MovementRestrictionWhileAttacking.jumpRestricted)
            {
                return false;
            }
            else if (IsUsingSkill && MovementRestrictionWhileUsingSkill.jumpRestricted)
            {
                return false;
            }
            else if (IsReloading && MovementRestrictionWhileReloading.jumpRestricted)
            {
                return false;
            }
            else if (IsCharging && MovementRestrictionWhileCharging.jumpRestricted)
            {
                return false;
            }
            return true;
        }

        protected override bool CanTurn_Implementation()
        {
            if (IsAttacking && MovementRestrictionWhileAttacking.turnRestricted)
            {
                return false;
            }
            else if (IsUsingSkill && MovementRestrictionWhileUsingSkill.turnRestricted)
            {
                return false;
            }
            else if (IsReloading && MovementRestrictionWhileReloading.turnRestricted)
            {
                return false;
            }
            else if (IsCharging && MovementRestrictionWhileCharging.turnRestricted)
            {
                return false;
            }
            return true;
        }

        protected override void OnTeleport(Vector3 position, Quaternion rotation)
        {
            base.OnTeleport(position, rotation);
            // Clear target entity when teleport
            SetTargetEntity(null);
            // Setup ground check data
            _lastGrounded = true;
            _lastGroundedPosition = position;
        }
    }
}