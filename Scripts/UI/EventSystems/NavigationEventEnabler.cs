using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class NavigationEventEnabler : MonoBehaviour
    {
        private void OnEnable()
        {
            EventSystem system = FindObjectOfType<EventSystem>();
            system.sendNavigationEvents = true;
        }

        private void OnDisable()
        {
            EventSystem system = FindObjectOfType<EventSystem>();
            system.sendNavigationEvents = false;
        }
    }
}
