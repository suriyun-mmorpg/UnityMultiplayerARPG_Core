using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientUserHandlers : MonoBehaviour, IClientUserHandlers
    {
        public string UserId { get { return BasePlayerCharacterController.OwningCharacter.UserId; } set { } }
        public string CharacterId { get { return BasePlayerCharacterController.OwningCharacter.Id; } set { } }
    }
}
