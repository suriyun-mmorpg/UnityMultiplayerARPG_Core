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
        public NpcDialog startDialog;
    }
}
