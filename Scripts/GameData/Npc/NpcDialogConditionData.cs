using UnityEngine;
using System.Collections;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class NpcDialogConditionData : SerializableCallback<IPlayerCharacterData, bool>
    {
    }
}
