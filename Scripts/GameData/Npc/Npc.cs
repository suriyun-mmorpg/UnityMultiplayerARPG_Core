using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct Npc
    {
        public string title;
        public Vector3 position;
        public Vector3 rotation;
        public NpcDialog startDialog;
        public CharacterModel characterModel;
    }
}
