using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationComponent : BaseCharacterComponent
{
    public const float UPDATE_VELOCITY_DURATION = 0.1f;

    #region Animation System Data
    [HideInInspector, System.NonSerialized]
    public Vector3? previousPosition;
    [HideInInspector, System.NonSerialized]
    public Vector3 currentVelocity;
    [HideInInspector, System.NonSerialized]
    public float velocityCalculationDeltaTime;
    #endregion

    protected void Update()
    {
        var deltaTime = Time.unscaledDeltaTime;
        var gameplayRule = GameInstance.Singleton.GameplayRule;
        UpdateAnimation(deltaTime, gameplayRule, this, CacheCharacterEntity, CacheCharacterEntity.CacheTransform);
    }

    protected static void UpdateAnimation(float deltaTime, BaseGameplayRule gameplayRule, CharacterAnimationComponent animationData, BaseCharacterEntity characterEntity, Transform transform)
    {
        if (characterEntity.isRecaching)
            return;

        // Update current velocity
        animationData.velocityCalculationDeltaTime += deltaTime;
        if (animationData.velocityCalculationDeltaTime >= UPDATE_VELOCITY_DURATION)
        {
            if (!animationData.previousPosition.HasValue)
                animationData.previousPosition = transform.position;
            var currentMoveDistance = transform.position - animationData.previousPosition.Value;
            animationData.currentVelocity = currentMoveDistance / animationData.velocityCalculationDeltaTime;
            animationData.previousPosition = transform.position;
            animationData.velocityCalculationDeltaTime = 0f;
        }

        var model = characterEntity.Model;
        if (model != null)
            model.UpdateAnimation(characterEntity.CurrentHp <= 0, animationData.currentVelocity, gameplayRule.GetMoveSpeed(characterEntity) / characterEntity.CacheBaseMoveSpeed);
    }
}
