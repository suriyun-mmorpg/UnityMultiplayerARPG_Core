using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseNpcDialogAction : ScriptableObject
    {
        public abstract void DoAction(IPlayerCharacterData playerCharacterEntity);
    }
}
