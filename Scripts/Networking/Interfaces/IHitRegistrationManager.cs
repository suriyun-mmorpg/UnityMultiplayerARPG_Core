using UnityEngine;

namespace MultiplayerARPG
{
    public interface IHitRegistrationManager
    {
        void PerformHitRegistration(BasePlayerCharacterEntity character, Vector3 origin, Vector3 direction, DamageableEntity target, byte hitBoxIndex, Vector3 hitPoint);
    }
}
