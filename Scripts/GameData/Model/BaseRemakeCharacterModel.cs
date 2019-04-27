using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseRemakeCharacterModel : BaseCharacterModel
    {
        // Clip name variables
        public const string CLIP_IDLE = "__Idle";
        public const string CLIP_MOVE = "__MoveForward";
        public const string CLIP_MOVE_BACKWARD = "__MoveBackward";
        public const string CLIP_MOVE_LEFT = "__MoveLeft";
        public const string CLIP_MOVE_RIGHT = "__MoveRight";
        public const string CLIP_MOVE_FORWARD_LEFT = "__MoveForwardLeft";
        public const string CLIP_MOVE_FORWARD_RIGHT = "__MoveForwardRight";
        public const string CLIP_MOVE_BACKWARD_LEFT = "__MoveBackwardLeft";
        public const string CLIP_MOVE_BACKWARD_RIGHT = "__MoveBackwardRight";
        public const string CLIP_JUMP = "__Jump";
        public const string CLIP_FALL = "__Fall";
        public const string CLIP_HURT = "__Hurt";
        public const string CLIP_DEAD = "__Dead";
        public const string CLIP_ACTION = "__Action";
        public const string CLIP_CAST_SKILL = "__CastSkill";

        [Header("Renderer")]
        [Tooltip("This will be used to apply bone weights when equip an equipments")]
        public SkinnedMeshRenderer skinnedMeshRenderer;

        [Header("Animations")]
        public DefaultAnimations defaultAnimations;
        public WeaponAnimations[] weaponAnimations;
        public SkillAnimations[] skillAnimations;

        private Dictionary<int, WeaponAnimations> cacheWeaponAnimations;
        public Dictionary<int, WeaponAnimations> CacheWeaponAnimations
        {
            get
            {
                if (cacheWeaponAnimations == null)
                {
                    cacheWeaponAnimations = new Dictionary<int, WeaponAnimations>();
                    foreach (WeaponAnimations weaponAnimation in weaponAnimations)
                    {
                        if (weaponAnimation.weaponType == null) continue;
                        cacheWeaponAnimations[weaponAnimation.weaponType.DataId] = weaponAnimation;
                    }
                }
                return cacheWeaponAnimations;
            }
        }

        private Dictionary<int, SkillAnimations> cacheSkillAnimations;
        public Dictionary<int, SkillAnimations> CacheSkillAnimations
        {
            get
            {
                if (cacheSkillAnimations == null)
                {
                    cacheSkillAnimations = new Dictionary<int, SkillAnimations>();
                    foreach (SkillAnimations skillAnimation in skillAnimations)
                    {
                        if (skillAnimation.skill == null) continue;
                        cacheSkillAnimations[skillAnimation.skill.DataId] = skillAnimation;
                    }
                }
                return cacheSkillAnimations;
            }
        }

        public override void AddingNewModel(GameObject newModel)
        {
            base.AddingNewModel(newModel);
            SkinnedMeshRenderer skinnedMesh = newModel.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh != null && skinnedMeshRenderer != null)
            {
                skinnedMesh.bones = skinnedMeshRenderer.bones;
                skinnedMesh.rootBone = skinnedMeshRenderer.rootBone;
            }
        }

        public ActionAnimation GetActionAnimation(AnimActionType animActionType, int dataId, int index)
        {
            ActionAnimation tempActionAnimation = default(ActionAnimation);
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    tempActionAnimation = GetRightHandAttackAnimations(dataId)[index];
                    break;
                case AnimActionType.AttackLeftHand:
                    tempActionAnimation = GetLeftHandAttackAnimations(dataId)[index];
                    break;
                case AnimActionType.Skill:
                    tempActionAnimation = GetSkillActivateAnimation(dataId);
                    break;
                case AnimActionType.ReloadRightHand:
                    tempActionAnimation = GetRightHandReloadAnimation(dataId);
                    break;
                case AnimActionType.ReloadLeftHand:
                    tempActionAnimation = GetLeftHandReloadAnimation(dataId);
                    break;
            }
            return tempActionAnimation;
        }

        public ActionAnimation[] GetRightHandAttackAnimations(WeaponType weaponType)
        {
            return GetRightHandAttackAnimations(weaponType.DataId);
        }

        public ActionAnimation[] GetRightHandAttackAnimations(int dataId)
        {
            if (CacheWeaponAnimations.ContainsKey(dataId) &&
                CacheWeaponAnimations[dataId].rightHandAttackAnimations != null)
                return CacheWeaponAnimations[dataId].rightHandAttackAnimations;
            return defaultAnimations.rightHandAttackAnimations;
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(WeaponType weaponType)
        {
            return GetLeftHandAttackAnimations(weaponType.DataId);
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(int dataId)
        {
            if (CacheWeaponAnimations.ContainsKey(dataId) &&
                CacheWeaponAnimations[dataId].leftHandAttackAnimations != null)
                return CacheWeaponAnimations[dataId].leftHandAttackAnimations;
            return defaultAnimations.leftHandAttackAnimations;
        }

        public AnimationClip GetSkillCastClip(int dataId)
        {
            if (CacheSkillAnimations.ContainsKey(dataId))
                return CacheSkillAnimations[dataId].castClip;
            return defaultAnimations.skillCastClip;
        }

        public ActionAnimation GetSkillActivateAnimation(int dataId)
        {
            if (CacheSkillAnimations.ContainsKey(dataId))
                return CacheSkillAnimations[dataId].activateAnimation;
            return defaultAnimations.skillActivateAnimation;
        }

        public ActionAnimation GetRightHandReloadAnimation(int dataId)
        {
            if (CacheWeaponAnimations.ContainsKey(dataId))
                return CacheWeaponAnimations[dataId].rightHandReloadAnimation;
            return defaultAnimations.rightHandReloadAnimation;
        }

        public ActionAnimation GetLeftHandReloadAnimation(int dataId)
        {
            if (CacheWeaponAnimations.ContainsKey(dataId))
                return CacheWeaponAnimations[dataId].leftHandReloadAnimation;
            return defaultAnimations.leftHandReloadAnimation;
        }

        public override bool GetRandomRightHandAttackAnimation(
            int dataId,
            out int animationIndex,
            out float triggerDuration,
            out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetRightHandAttackAnimations(dataId);
            animationIndex = 0;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0) return false;
            animationIndex = Random.Range(0, tempActionAnimations.Length);
            triggerDuration = tempActionAnimations[animationIndex].GetTriggerDuration();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetRandomLeftHandAttackAnimation(
            int dataId,
            out int animationIndex,
            out float triggerDuration,
            out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetLeftHandAttackAnimations(dataId);
            animationIndex = 0;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0) return false;
            animationIndex = Random.Range(0, tempActionAnimations.Length);
            triggerDuration = tempActionAnimations[animationIndex].GetTriggerDuration();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetSkillActivateAnimation(
            int dataId,
            out float triggerDuration,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetSkillActivateAnimation(dataId);
            triggerDuration = tempActionAnimation.GetTriggerDuration();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetRightHandReloadAnimation(
            int dataId,
            out float triggerDuration,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetRightHandReloadAnimation(dataId);
            triggerDuration = tempActionAnimation.GetTriggerDuration();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetLeftHandReloadAnimation(
            int dataId,
            out float triggerDuration,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetLeftHandReloadAnimation(dataId);
            triggerDuration = tempActionAnimation.GetTriggerDuration();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override SkillActivateAnimationType UseSkillActivateAnimationType(int dataId)
        {
            if (!CacheSkillAnimations.ContainsKey(dataId))
                return SkillActivateAnimationType.UseAttackAnimation;
            return CacheSkillAnimations[dataId].activateAnimationType;
        }
    }
}
