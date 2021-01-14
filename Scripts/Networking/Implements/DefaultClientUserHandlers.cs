using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientUserHandlers : MonoBehaviour, IClientUserHandlers
    {
        public string UserId { get; set; }
        public string UserToken { get; set; }
        public string CharacterId { get; set; }
    }
}
