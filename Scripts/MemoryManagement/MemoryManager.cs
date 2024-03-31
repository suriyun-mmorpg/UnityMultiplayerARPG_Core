using UnityEngine;

namespace MultiplayerARPG
{
    public class MemoryManager : MonoBehaviour
    {

        private static MemoryManager s_Instance;
        public static MemoryManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new GameObject("_MemoryManager").AddComponent<MemoryManager>();
                    DontDestroyOnLoad(s_Instance.gameObject);
                }
                return s_Instance;
            }
        }

        public static CharacterBuffCacheManager CharacterBuffs => Instance._characterBuffs;
        public static CharacterItemCacheManager CharacterItems => Instance._characterItems;
        public static CharacterSummonCacheManager CharacterSummons => Instance._characterSummons;

        public float updateDelay = 10f;

        private float _lastUpdateTime;
        private readonly CharacterBuffCacheManager _characterBuffs = new CharacterBuffCacheManager();
        private readonly CharacterItemCacheManager _characterItems = new CharacterItemCacheManager();
        private readonly CharacterSummonCacheManager _characterSummons = new CharacterSummonCacheManager();

        private void Update()
        {
            float time = Time.unscaledTime;
            if (time - _lastUpdateTime < updateDelay)
                return;
            // Update other sub cache managers
            CharacterBuffs.OnUpdate();
            CharacterItems.OnUpdate();
            CharacterSummons.OnUpdate();
            _lastUpdateTime = time;
        }

        private void OnDestroy()
        {
            // Clear sub cache managers
            CharacterBuffs.Clear();
            CharacterItems.Clear();
            CharacterSummons.Clear();
        }
    }
}