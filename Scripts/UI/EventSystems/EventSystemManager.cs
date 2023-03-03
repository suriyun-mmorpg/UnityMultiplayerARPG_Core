using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    public static class EventSystemManager
    {
        public static EventSystem CurrentEventSystem;
        public static event System.Action onEventSystemReady;

        static EventSystemManager()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            CurrentEventSystem = Object.FindObjectOfType<EventSystem>();
            // Create a new event system
            if (CurrentEventSystem == null)
            {
                CurrentEventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                CurrentEventSystem.gameObject.GetOrAddComponent<InputSystemUIInputModule>();
#else
                CurrentEventSystem.gameObject.GetOrAddComponent<StandaloneInputModule>();
#endif
            }

#if ENABLE_INPUT_SYSTEM
            StandaloneInputModule oldInputModule = CurrentEventSystem.GetComponent<StandaloneInputModule>();
            if (oldInputModule != null)
                Object.DestroyImmediate(oldInputModule);
            CurrentEventSystem.gameObject.GetOrAddComponent<InputSystemUIInputModule>();
#endif
            CurrentEventSystem.sendNavigationEvents = false;

            if (onEventSystemReady != null)
                onEventSystemReady.Invoke();
        }
    }
}
