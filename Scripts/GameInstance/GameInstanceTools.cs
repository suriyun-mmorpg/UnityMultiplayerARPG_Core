using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [RequireComponent(typeof(GameInstance))]
    public class GameInstanceTools : MonoBehaviour
    {
        [Header("Exp calculator")]
        public short maxLevel;
        public Int32GraphCalculator expCalculator;
        public bool calculateExp;

        private GameInstance cacheGameInstance;
        public GameInstance CacheGameInstance
        {
            get
            {
                if (cacheGameInstance == null)
                    cacheGameInstance = GetComponent<GameInstance>();
                return cacheGameInstance;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (calculateExp)
            {
                calculateExp = false;
                var expTree = new int[maxLevel];
                for (short i = 1; i <= maxLevel; ++i)
                {
                    expTree[i - 1] = expCalculator.Calculate(i, maxLevel);
                }
                CacheGameInstance.ExpTree = expTree;
                EditorUtility.SetDirty(CacheGameInstance);
            }
        }
#endif
    }
}
