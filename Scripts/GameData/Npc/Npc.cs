using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct Npc
    {
        public NpcEntity entityPrefab;
        public Vector3 position;
        public Vector3 rotation;
        public string title;
        [Tooltip("It will use `startDialog` if `graph` is empty")]
        public BaseNpcDialog startDialog;
        [Tooltip("It will use `graph` start dialog if this is not empty")]
        public NpcDialogGraph graph;
    }
}
