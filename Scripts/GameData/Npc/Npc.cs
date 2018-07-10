using UnityEngine;
namespace MultiplayerARPG
{
    [System.Serializable]
    public struct Npc
    {
        public Vector3 position;
        public Vector3 rotation;
        public string title;
        public NpcDialog startDialog;
        public CharacterModel model;
    }
}
