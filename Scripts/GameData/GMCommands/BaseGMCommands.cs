using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseGMCommands : ScriptableObject
    {
        public abstract bool IsGMCommand(string chatMessage, out string command);
        public abstract bool CanUseGMCommand(BasePlayerCharacterEntity characterEntity, string command);
        public abstract void HandleGMCommand(string sender, string chatMessage);
    }
}
