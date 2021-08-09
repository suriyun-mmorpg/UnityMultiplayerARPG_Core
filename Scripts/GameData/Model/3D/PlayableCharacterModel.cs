using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace MultiplayerARPG.GameData.Model.Playables
{
    public class PlayableCharacterModel : BaseCharacterModel
    {
        public PlayableGraph Graph { get; protected set; }
        protected override void Awake()
        {
            base.Awake();
        }

        private void OnDestroy()
        {
            Graph.Destroy();
        }

        #region Right-hand animations
        public override bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetRightHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetRandomRightHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Left-hand animations
        public override bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetLeftHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetRandomLeftHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Skill animations
        public override bool GetSkillActivateAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override SkillActivateAnimationType UseSkillActivateAnimationType(int dataId)
        {
            throw new System.NotImplementedException();
        }

        public override Coroutine PlaySkillCastClip(int dataId, float duration)
        {
            throw new System.NotImplementedException();
        }

        public override void StopSkillCastAnimation()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Action animations
        public override Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1)
        {
            throw new System.NotImplementedException();
        }

        public override void StopActionAnimation()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Weapon charge animations
        public override void PlayWeaponChargeClip(int dataId, bool isLeftHand)
        {
            throw new System.NotImplementedException();
        }

        public override void StopWeaponChargeAnimation()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Other animations
        public override void PlayMoveAnimation()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
