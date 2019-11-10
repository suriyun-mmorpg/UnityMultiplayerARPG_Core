using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract class BaseRemakeCharacterModel : BaseCharacterModelWithCacheAnims<WeaponAnimations, SkillAnimations>
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
        [ArrayElementTitle("weaponType", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public WeaponAnimations[] weaponAnimations;
        [ArrayElementTitle("skill", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public SkillAnimations[] skillAnimations;

        public override void AddingNewModel(GameObject newModel, EquipmentContainer equipmentContainer)
        {
            base.AddingNewModel(newModel, equipmentContainer);
            SkinnedMeshRenderer skinnedMesh = newModel.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh != null && skinnedMeshRenderer != null)
            {
                skinnedMesh.bones = skinnedMeshRenderer.bones;
                skinnedMesh.rootBone = skinnedMeshRenderer.rootBone;
                if (equipmentContainer.defaultModel != null)
                {
                    SkinnedMeshRenderer defaultSkinnedMesh = equipmentContainer.defaultModel.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (defaultSkinnedMesh != null)
                    {
                        skinnedMesh.bones = defaultSkinnedMesh.bones;
                        skinnedMesh.rootBone = defaultSkinnedMesh.rootBone;
                    }
                }
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
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
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
            if (GetAnims().CacheWeaponAnimations.ContainsKey(dataId) &&
                GetAnims().CacheWeaponAnimations[dataId].rightHandAttackAnimations != null)
                return GetAnims().CacheWeaponAnimations[dataId].rightHandAttackAnimations;
            return defaultAnimations.rightHandAttackAnimations;
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(WeaponType weaponType)
        {
            return GetLeftHandAttackAnimations(weaponType.DataId);
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(int dataId)
        {
            if (GetAnims().CacheWeaponAnimations.ContainsKey(dataId) &&
                GetAnims().CacheWeaponAnimations[dataId].leftHandAttackAnimations != null)
                return GetAnims().CacheWeaponAnimations[dataId].leftHandAttackAnimations;
            return defaultAnimations.leftHandAttackAnimations;
        }

        public AnimationClip GetSkillCastClip(int dataId)
        {
            if (GetAnims().CacheSkillAnimations.ContainsKey(dataId) &&
                GetAnims().CacheSkillAnimations[dataId].castClip != null)
                return GetAnims().CacheSkillAnimations[dataId].castClip;
            return defaultAnimations.skillCastClip;
        }

        public ActionAnimation GetSkillActivateAnimation(int dataId)
        {
            if (GetAnims().CacheSkillAnimations.ContainsKey(dataId) &&
                GetAnims().CacheSkillAnimations[dataId].activateAnimation.clip != null)
                return GetAnims().CacheSkillAnimations[dataId].activateAnimation;
            return defaultAnimations.skillActivateAnimation;
        }

        public ActionAnimation GetRightHandReloadAnimation(int dataId)
        {
            if (GetAnims().CacheWeaponAnimations.ContainsKey(dataId) &&
                GetAnims().CacheWeaponAnimations[dataId].rightHandReloadAnimation.clip != null)
                return GetAnims().CacheWeaponAnimations[dataId].rightHandReloadAnimation;
            return defaultAnimations.rightHandReloadAnimation;
        }

        public ActionAnimation GetLeftHandReloadAnimation(int dataId)
        {
            if (GetAnims().CacheWeaponAnimations.ContainsKey(dataId) &&
                GetAnims().CacheWeaponAnimations[dataId].leftHandReloadAnimation.clip != null)
                return GetAnims().CacheWeaponAnimations[dataId].leftHandReloadAnimation;
            return defaultAnimations.leftHandReloadAnimation;
        }

        public override bool GetRandomRightHandAttackAnimation(
            int dataId,
            out int animationIndex,
            out float[] triggerDurations,
            out float totalDuration)
        {
            animationIndex = Random.Range(0, GetRightHandAttackAnimations(dataId).Length);
            return GetRightHandAttackAnimation(dataId, animationIndex, out triggerDurations, out totalDuration);
        }

        public override bool GetRandomLeftHandAttackAnimation(
            int dataId,
            out int animationIndex,
            out float[] triggerDurations,
            out float totalDuration)
        {
            animationIndex = Random.Range(0, GetLeftHandAttackAnimations(dataId).Length);
            return GetLeftHandAttackAnimation(dataId, animationIndex, out triggerDurations, out totalDuration);
        }

        public override bool GetRightHandAttackAnimation(
            int dataId,
            int animationIndex,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetRightHandAttackAnimations(dataId);
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0 || animationIndex >= tempActionAnimations.Length) return false;
            triggerDurations = tempActionAnimations[animationIndex].GetTriggerDurations();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetLeftHandAttackAnimation(
            int dataId,
            int animationIndex,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetLeftHandAttackAnimations(dataId);
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0 || animationIndex >= tempActionAnimations.Length) return false;
            triggerDurations = tempActionAnimations[animationIndex].GetTriggerDurations();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetSkillActivateAnimation(
            int dataId,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetSkillActivateAnimation(dataId);
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetRightHandReloadAnimation(
            int dataId,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetRightHandReloadAnimation(dataId);
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetLeftHandReloadAnimation(
            int dataId,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetLeftHandReloadAnimation(dataId);
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override SkillActivateAnimationType UseSkillActivateAnimationType(int dataId)
        {
            if (!GetAnims().CacheSkillAnimations.ContainsKey(dataId))
                return SkillActivateAnimationType.UseActivateAnimation;
            return GetAnims().CacheSkillAnimations[dataId].activateAnimationType;
        }

        protected override WeaponAnimations[] GetWeaponAnims()
        {
            return weaponAnimations;
        }

        protected override SkillAnimations[] GetSkillAnims()
        {
            return skillAnimations;
        }

#if UNITY_EDITOR
        [ContextMenu("Copy Weapon Animations")]
        public void CopyWeaponAnimations()
        {
            CharacterModelDataManager.CopyWeaponAnimations(weaponAnimations);
        }

        [ContextMenu("Paste Weapon Animations")]
        public void PasteWeaponAnimations()
        {
            WeaponAnimations[] weaponAnimations = CharacterModelDataManager.PasteWeaponAnimations();
            if (weaponAnimations != null)
            {
                this.weaponAnimations = weaponAnimations;
                EditorUtility.SetDirty(this);
            }
        }

        [ContextMenu("Copy Skill Animations")]
        public void CopySkillAnimations()
        {
            CharacterModelDataManager.CopySkillAnimations(skillAnimations);
        }

        [ContextMenu("Paste Skill Animations")]
        public void PasteSkillAnimations()
        {
            SkillAnimations[] skillAnimations = CharacterModelDataManager.PasteSkillAnimations();
            if (skillAnimations != null)
            {
                this.skillAnimations = skillAnimations;
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
