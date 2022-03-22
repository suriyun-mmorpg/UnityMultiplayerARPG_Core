using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultHitRegistrationManager : MonoBehaviour, IHitRegistrationManager
    {
        public void PerformHitRegistration(BasePlayerCharacterEntity character, Vector3 origin, Vector3 direction, DamageableEntity target, byte hitBoxIndex, Vector3 hitPoint)
        {
            // Yes, it is hit.
        }
    }
}
