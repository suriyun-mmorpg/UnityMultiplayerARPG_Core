using UnityEngine;

namespace MultiplayerARPG
{
    public interface ITargetableEntity
    {
        Transform EntityTransform { get; }
        GameObject EntityGameObject { get; }
        /// <summary>
        /// This must return `TRUE`, if you want controller to set this entity as target immediately in one click. if it is `FAKSE`, first click will select it for information viewing, second will set it as target.
        /// </summary>
        /// <returns></returns>
        bool ShouldSetAsTargetInOneClick();
    }
}
