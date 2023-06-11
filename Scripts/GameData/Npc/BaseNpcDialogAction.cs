using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseNpcDialogAction : ScriptableObject
    {
        public virtual bool PassCondition(IPlayerCharacterData playerCharacterEntity, out UITextKeys message)
        {
            message = UITextKeys.NONE;
            return false;
        }
        public abstract void DoAction(IPlayerCharacterData playerCharacterEntity);
    }
}
