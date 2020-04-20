using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseGMCommands : ScriptableObject
    {
        public abstract bool IsGMCommand(string chatMessage);
        public abstract void HandleGMCommand(BaseGameNetworkManager manager, string sender, string command);
    }
}
