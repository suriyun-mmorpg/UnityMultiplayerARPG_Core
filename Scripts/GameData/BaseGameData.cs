using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameData : ScriptableObject
    {
        public string title;
        [TextArea]
        public string description;
        public Sprite icon;
        
        public virtual string Id { get { return name; } }
        public virtual string Title { get { return title; } }
        public virtual string Description { get { return description; } }
        public int DataId { get { return Id.GenerateHashId(); } }
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }
    }
}
