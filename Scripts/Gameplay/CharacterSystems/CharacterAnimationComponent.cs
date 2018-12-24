using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterAnimationComponent : BaseCharacterComponent
    {
        public const float UPDATE_VELOCITY_DURATION = 0.1f;

        #region Animation System Data
        [HideInInspector, System.NonSerialized]
        public Vector3? previousPosition;
        [HideInInspector, System.NonSerialized]
        public Vector3 currentVelocity;
        [HideInInspector, System.NonSerialized]
        public float updatingTime;
        #endregion

        private static BaseCharacterModel tempCharacterModel;

        protected void Update()
        {
            UpdateAnimation(Time.unscaledDeltaTime, this, CacheCharacterEntity, CacheCharacterEntity.CacheTransform);
        }

        protected static void UpdateAnimation(float deltaTime, CharacterAnimationComponent animationData, BaseCharacterEntity characterEntity, Transform transform)
        {
            if (characterEntity.isRecaching)
                return;

            // Update current velocity
            animationData.updatingTime += deltaTime;
            if (animationData.updatingTime >= UPDATE_VELOCITY_DURATION)
            {
                if (!animationData.previousPosition.HasValue)
                    animationData.previousPosition = transform.position;
                animationData.currentVelocity = (transform.position - animationData.previousPosition.Value) / animationData.updatingTime;
                animationData.previousPosition = transform.position;
                animationData.updatingTime = 0f;
            }

            tempCharacterModel = characterEntity.CharacterModel;
            if (tempCharacterModel != null)
                tempCharacterModel.UpdateAnimation(characterEntity.IsDead(), animationData.currentVelocity, characterEntity.MoveAnimationSpeedMultiplier);
        }
    }
}
