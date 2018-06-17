using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class CharacterAnimationSystem : ComponentSystem
{
    public const string ANIM_STATE_ACTION_CLIP = "_Action";
    public static readonly int ANIM_IS_DEAD = Animator.StringToHash("IsDead");
    public static readonly int ANIM_MOVE_SPEED = Animator.StringToHash("MoveSpeed");
    public static readonly int ANIM_Y_SPEED = Animator.StringToHash("YSpeed");
    public static readonly int ANIM_DO_ACTION = Animator.StringToHash("DoAction");
    public static readonly int ANIM_HURT = Animator.StringToHash("Hurt");
    public static readonly int ANIM_MOVE_CLIP_MULTIPLIER = Animator.StringToHash("MoveSpeedMultiplier");
    public static readonly int ANIM_ACTION_CLIP_MULTIPLIER = Animator.StringToHash("ActionSpeedMultiplier");
    public const float UPDATE_VELOCITY_DURATION = 0.1f;

    struct Components
    {
        public CharacterAnimationData animationData;
        public Transform transform;
    }

    protected override void OnUpdate()
    {
        var deltaTime = Time.unscaledDeltaTime;
        var gameInstance = GameInstance.Singleton;
        var gameplayRule = gameInstance != null ? gameInstance.GameplayRule : null;
        foreach (var comp in GetEntities<Components>())
        {
            UpdateAnimation(deltaTime, gameplayRule, comp.animationData, comp.animationData.CacheCharacterEntity, comp.transform);
        }
    }

    protected static void UpdateAnimation(float deltaTime, BaseGameplayRule gameplayRule, CharacterAnimationData animationData, BaseCharacterEntity characterEntity, Transform transform)
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

        var modelAnimator = characterEntity.ModelAnimator;
        if (modelAnimator != null && modelAnimator.isActiveAndEnabled)
        {
            if (characterEntity.CurrentHp <= 0 && modelAnimator.GetBool(ANIM_DO_ACTION))
            {
                // Force set to none action when dead
                modelAnimator.SetBool(ANIM_DO_ACTION, false);
            }
            modelAnimator.SetFloat(ANIM_MOVE_SPEED, characterEntity.CurrentHp <= 0 ? 0 : new Vector3(animationData.currentVelocity.x, 0, animationData.currentVelocity.z).magnitude);
            modelAnimator.SetFloat(ANIM_MOVE_CLIP_MULTIPLIER, gameplayRule.GetMoveSpeed(characterEntity) / characterEntity.CacheBaseMoveSpeed);
            modelAnimator.SetFloat(ANIM_Y_SPEED, animationData.currentVelocity.y);
            modelAnimator.SetBool(ANIM_IS_DEAD, characterEntity.CurrentHp <= 0);
        }
    }
}
