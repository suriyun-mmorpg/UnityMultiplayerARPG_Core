using UnityEngine;

namespace MultiplayerARPG
{
    public class AnimatorHandle : MonoBehaviour
    {
        public int Id { get; private set; }
        private static int _nextId = 1;
        public System.Action<AnimatorHandle> OnDestroyed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetInstance()
        {
            _nextId = 1;
        }

        void Awake()
        {
            Id = _nextId++;
        }

        void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
            OnDestroyed = null;
        }
    }
}