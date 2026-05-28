using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.NPC_DATABASE_FILE, menuName = GameDataMenuConsts.NPC_DATABASE_MENU, order = GameDataMenuConsts.NPC_DATABASE_ORDER)]
    public class NpcDatabase : ScriptableObject
    {
        public Npcs[] maps = new Npcs[0];

#if UNITY_EDITOR
        private void OnValidate()
        {
            bool hasChanges = false;
            hasChanges |= ValidateAddressableHashAssetIDs();
            if (hasChanges)
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

        public bool ValidateAddressableHashAssetIDs()
        {
            bool hasChanges = false;
            foreach (Npcs npcMap in maps)
            {
                hasChanges |= npcMap.ValidateAddressableHashAssetIDs();
            }
            return hasChanges;
        }
#endif
    }
}
