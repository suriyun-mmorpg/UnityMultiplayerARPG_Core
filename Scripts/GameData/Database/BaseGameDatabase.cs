using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseGameDatabase : ScriptableObject
    {
        public abstract void LoadData(GameInstance gameInstance);
    }
}
