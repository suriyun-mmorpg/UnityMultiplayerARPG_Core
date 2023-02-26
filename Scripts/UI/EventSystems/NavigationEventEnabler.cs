using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class NavigationEventEnabler : MonoBehaviour
    {
        public bool setupOnEnable;
        public bool setupOnDisable;

        private void OnEnable()
        {
            if (setupOnEnable)
                SetSendNavigationEvents(true);
        }

        private void OnDisable()
        {
            if (setupOnDisable)
                SetSendNavigationEvents(false);
        }

        public void SetSendNavigationEvents(bool enable)
        {
            EventSystem system = FindObjectOfType<EventSystem>();
            if (system != null)
                system.sendNavigationEvents = enable;
        }
    }
}
