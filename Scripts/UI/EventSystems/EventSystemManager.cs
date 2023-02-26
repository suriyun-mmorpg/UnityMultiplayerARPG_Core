using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    public class EventSystemManager : MonoBehaviour
    {
        public static EventSystemManager Instance { get; private set; }
        public static EventSystem CurrentEventSystem;
        public event System.Action onEventSystemReady;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            CurrentEventSystem = FindObjectOfType<EventSystem>();
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
                DestroyImmediate(oldInputModule);
            CurrentEventSystem.gameObject.GetOrAddComponent<InputSystemUIInputModule>();
#endif
            CurrentEventSystem.sendNavigationEvents = false;

            if (onEventSystemReady != null)
                onEventSystemReady.Invoke();
        }
    }
}
