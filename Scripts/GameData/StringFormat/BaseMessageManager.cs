using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseMessageManager : ScriptableObject
    {
        public abstract string ReplaceMessageKeys(string format);
    }
}