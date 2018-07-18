using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameData : ScriptableObject
    {
        public string title;
        [TextArea]
        public string description;
        public Sprite icon;

        public string Id { get { return name; } }
        public int DataId { get { return Id.GenerateHashId(); } }
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }
    }
}
