using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class LanGameEnableDisableEvents : MonoBehaviour
    {
        public UnityEvent onEnable = new UnityEvent();
        public UnityEvent onDisable = new UnityEvent();

        private void OnEnable()
        {
            onEnable.Invoke();
        }

        private void OnDisable()
        {
            onDisable.Invoke();
        }
    }
}
