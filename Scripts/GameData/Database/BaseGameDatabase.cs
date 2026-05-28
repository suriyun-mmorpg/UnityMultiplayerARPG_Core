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
            await LoadDataImplement(gameInstance);
            // Tell game instance that data loaded
            gameInstance.LoadedGameData();
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
