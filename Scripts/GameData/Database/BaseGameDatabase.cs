using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract class BaseGameDatabase : ScriptableObject
    {
        public async UniTaskVoid LoadData(GameInstance gameInstance)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var loading = System.Diagnostics.Stopwatch.StartNew();
#endif
            await LoadDataImplement(gameInstance);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            double databaseMilliseconds = loading.Elapsed.TotalMilliseconds;
#endif
            // Start patch IO now; bundled data can open the home/login scene immediately.
            if (this is GameDatabase database)
                DataPatchRuntime.StartInitialization(gameInstance.DataPatchProfile, database);
            // Tell game instance that data loaded
            gameInstance.LoadedGameData();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[Game Data] Initialization finished in {loading.Elapsed.TotalMilliseconds:F1} ms: bundled database {databaseMilliseconds:F1} ms, final setup {loading.Elapsed.TotalMilliseconds - databaseMilliseconds:F1} ms.");
#endif
        }

        protected abstract UniTask LoadDataImplement(GameInstance gameInstance);

#if UNITY_EDITOR
        [ContextMenu("Force Validate")]
        public virtual bool Validate()
        {
            return false;
        }

        private void OnValidate()
        {
            if (Validate())
            {
                MarkDirty();
                Debug.Log($"Has changes on validate game database: {name}", this);
            }
        }

        private bool _queuedDirty;
        private void MarkDirty()
        {
            if (_queuedDirty)
                return;
            _queuedDirty = true;
            EditorApplication.delayCall += DelayedMarkDirty;
        }

        private void DelayedMarkDirty()
        {
            _queuedDirty = false;
            if (this == null)
                return;
            EditorApplication.delayCall -= DelayedMarkDirty;
            EditorUtility.SetDirty(this);
        }

        private void OnDestroy()
        {
            EditorApplication.delayCall -= DelayedMarkDirty;
        }
#endif
    }
}
