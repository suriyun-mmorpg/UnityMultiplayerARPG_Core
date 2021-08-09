using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MultiplayerARPG.GameData.Model.Playables
{
    /// <summary>
    /// NOTE: Set its name to default playable behaviour, in the future I might make it able to customize character model's playable behaviour
    /// </summary>
    public class DefaultPlayableBehaviour : PlayableBehaviour
    {
        public Playable Self { get; private set; }
        public PlayableGraph Graph { get; private set; }
        public AnimationLayerMixerPlayable LayerMixer { get; private set; }

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

        public void PlayAction()
        {

        }

        public void StopAction()
        {

        }

        public void PlaySkillCast()
        {

        }

        public void StopSkillCast()
        {

        }

        public void PlayWeaponCharge()
        {

        }

        public void StopWeaponCharge()
        {

        }
    }
}