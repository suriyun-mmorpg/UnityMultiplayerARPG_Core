using UnityEngine;
using MultiplayerARPG;

namespace MultiplayerARPG
{
    public class DeactivateIfServerStarted : MonoBehaviour
    {
        private void Update()
        {
            if (BaseGameNetworkManager.Singleton != null && BaseGameNetworkManager.Singleton.IsServer)
                gameObject.SetActive(false);
        }
    }
}
