using UnityEngine;

namespace MultiplayerARPG
{
    public interface IBaseActivatableEntity : ITargetableEntity
    {
        float GetActivatableDistance();
        /// <summary>
        /// If this returns `TRUE`, when set this entity as a target, character will move to it to attack. Otherwise it will move to it to activate.
        /// </summary>
        /// <returns></returns>
        bool ShouldBeAttackTarget();
        /// <summary>
        /// If this returns `TRUE`, when character moved to it and activated, it will clear player's target from controller.
        /// </summary>
        /// <returns></returns>
        bool ShouldClearTargetAfterActivated();
    }
}
