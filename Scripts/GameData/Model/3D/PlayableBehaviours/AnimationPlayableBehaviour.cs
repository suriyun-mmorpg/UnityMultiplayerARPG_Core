using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MultiplayerARPG.GameData.Model.Playables
{
    /// <summary>
    /// NOTE: Set its name to default playable behaviour, in the future I might make it able to customize character model's playable behaviour
    /// </summary>
    public class AnimationPlayableBehaviour : PlayableBehaviour
    {
        // Clip name variables
        // Move
        public const string CLIP_IDLE = "__Idle";
        public const string CLIP_MOVE = "__MoveForward";
        public const string CLIP_MOVE_BACKWARD = "__MoveBackward";
        public const string CLIP_MOVE_LEFT = "__MoveLeft";
        public const string CLIP_MOVE_RIGHT = "__MoveRight";
        public const string CLIP_MOVE_FORWARD_LEFT = "__MoveForwardLeft";
        public const string CLIP_MOVE_FORWARD_RIGHT = "__MoveForwardRight";
        public const string CLIP_MOVE_BACKWARD_LEFT = "__MoveBackwardLeft";
        public const string CLIP_MOVE_BACKWARD_RIGHT = "__MoveBackwardRight";
        // Sprint
        public const string CLIP_SPRINT = "__SprintForward";
        public const string CLIP_SPRINT_BACKWARD = "__SprintBackward";
        public const string CLIP_SPRINT_LEFT = "__SprintLeft";
        public const string CLIP_SPRINT_RIGHT = "__SprintRight";
        public const string CLIP_SPRINT_FORWARD_LEFT = "__SprintForwardLeft";
        public const string CLIP_SPRINT_FORWARD_RIGHT = "__SprintForwardRight";
        public const string CLIP_SPRINT_BACKWARD_LEFT = "__SprintBackwardLeft";
        public const string CLIP_SPRINT_BACKWARD_RIGHT = "__SprintBackwardRight";
        // Walk
        public const string CLIP_WALK = "__WalkForward";
        public const string CLIP_WALK_BACKWARD = "__WalkBackward";
        public const string CLIP_WALK_LEFT = "__WalkLeft";
        public const string CLIP_WALK_RIGHT = "__WalkRight";
        public const string CLIP_WALK_FORWARD_LEFT = "__WalkForwardLeft";
        public const string CLIP_WALK_FORWARD_RIGHT = "__WalkForwardRight";
        public const string CLIP_WALK_BACKWARD_LEFT = "__WalkBackwardLeft";
        public const string CLIP_WALK_BACKWARD_RIGHT = "__WalkBackwardRight";
        // Crouch
        public const string CLIP_CROUCH_IDLE = "__CrouchIdle";
        public const string CLIP_CROUCH_MOVE = "__CrouchMoveForward";
        public const string CLIP_CROUCH_MOVE_BACKWARD = "__CrouchMoveBackward";
        public const string CLIP_CROUCH_MOVE_LEFT = "__CrouchMoveLeft";
        public const string CLIP_CROUCH_MOVE_RIGHT = "__CrouchMoveRight";
        public const string CLIP_CROUCH_MOVE_FORWARD_LEFT = "__CrouchMoveForwardLeft";
        public const string CLIP_CROUCH_MOVE_FORWARD_RIGHT = "__CrouchMoveForwardRight";
        public const string CLIP_CROUCH_MOVE_BACKWARD_LEFT = "__CrouchMoveBackwardLeft";
        public const string CLIP_CROUCH_MOVE_BACKWARD_RIGHT = "__CrouchMoveBackwardRight";
        // Crawl
        public const string CLIP_CRAWL_IDLE = "__CrawlIdle";
        public const string CLIP_CRAWL_MOVE = "__CrawlMoveForward";
        public const string CLIP_CRAWL_MOVE_BACKWARD = "__CrawlMoveBackward";
        public const string CLIP_CRAWL_MOVE_LEFT = "__CrawlMoveLeft";
        public const string CLIP_CRAWL_MOVE_RIGHT = "__CrawlMoveRight";
        public const string CLIP_CRAWL_MOVE_FORWARD_LEFT = "__CrawlMoveForwardLeft";
        public const string CLIP_CRAWL_MOVE_FORWARD_RIGHT = "__CrawlMoveForwardRight";
        public const string CLIP_CRAWL_MOVE_BACKWARD_LEFT = "__CrawlMoveBackwardLeft";
        public const string CLIP_CRAWL_MOVE_BACKWARD_RIGHT = "__CrawlMoveBackwardRight";
        // Swim
        public const string CLIP_SWIM_IDLE = "__SwimIdle";
        public const string CLIP_SWIM_MOVE = "__SwimMoveForward";
        public const string CLIP_SWIM_MOVE_BACKWARD = "__SwimMoveBackward";
        public const string CLIP_SWIM_MOVE_LEFT = "__SwimMoveLeft";
        public const string CLIP_SWIM_MOVE_RIGHT = "__SwimMoveRight";
        public const string CLIP_SWIM_MOVE_FORWARD_LEFT = "__SwimMoveForwardLeft";
        public const string CLIP_SWIM_MOVE_FORWARD_RIGHT = "__SwimMoveForwardRight";
        public const string CLIP_SWIM_MOVE_BACKWARD_LEFT = "__SwimMoveBackwardLeft";
        public const string CLIP_SWIM_MOVE_BACKWARD_RIGHT = "__SwimMoveBackwardRight";
        // Other
        public const string CLIP_JUMP = "__Jump";
        public const string CLIP_FALL = "__Fall";
        public const string CLIP_LANDED = "__Landed";
        public const string CLIP_HURT = "__Hurt";
        public const string CLIP_DEAD = "__Dead";
        public const string CLIP_ACTION = "__Action";
        public const string CLIP_CAST_SKILL = "__CastSkill";
        public const string CLIP_WEAPON_CHARGE = "__WeaponCharge";
        public const string CLIP_PICKUP = "__Pickup";

        public Playable Self { get; private set; }
        public PlayableGraph Graph { get; private set; }
        public AnimationLayerMixerPlayable LayerMixer { get; private set; }
        public PlayableCharacterModel CharacterModel { get; private set; }

        public override void OnPlayableCreate(Playable playable)
        {
            Self = playable;
            Graph = playable.GetGraph();
            // Create layer mixer which have 2 layers as base layer and action layer
            LayerMixer = AnimationLayerMixerPlayable.Create(Graph, 2);
            // Connect layer mixer to this instance
            Graph.Connect(LayerMixer, 0, Self, 0);
            Self.SetInputCount(1);
            Self.SetInputWeight(0, 1);
        }

        public void Setup(PlayableCharacterModel characterModel)
        {
            CharacterModel = characterModel;
            // Setup clips by settings in character model
            // Default

            // Clips based on equipped weapons

        }

        public void SetMovementState(MovementState movementState)
        {

        }

        public void SetExtraMovementState(ExtraMovementState extraMovementState)
        {

        }

        public void PlayHit()
        {

        }

        public void PlayJump()
        {

        }

        public void PlayPickup()
        {

        }

        public void PlayAction(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {

        }

        public void StopAction()
        {

        }

        public void PlaySkillCast(int dataId, float duration)
        {

        }

        public void StopSkillCast()
        {

        }

        public void PlayWeaponCharge(int dataId, bool isLeftHand)
        {

        }

        public void StopWeaponCharge()
        {

        }
    }
}