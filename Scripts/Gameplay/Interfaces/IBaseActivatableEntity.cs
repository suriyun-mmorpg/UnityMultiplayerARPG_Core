using UnityEngine;

namespace MultiplayerARPG
{
    public interface IBaseActivatableEntity : ITargetableEntity
    {
        float GetActivatableDistance();
        bool ShouldBeAttackTarget();
        bool ShouldClearPlayerTargetAfterActivated();
    }
}
