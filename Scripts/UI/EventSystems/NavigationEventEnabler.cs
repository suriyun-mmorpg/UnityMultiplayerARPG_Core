using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class NavigationEventEnabler : MonoBehaviour
    {
        private void OnEnable()
        {
            EventSystem system = FindObjectOfType<EventSystem>();
            if (system != null)
                system.sendNavigationEvents = true;
        }

        private void OnDisable()
        {
            EventSystem system = FindObjectOfType<EventSystem>();
            if (system != null)
                system.sendNavigationEvents = false;
        }
    }
}
