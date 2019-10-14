using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract partial class BaseGameData : ScriptableObject
    {
        [Header("Game Data Configs")]
        [Tooltip("Default title")]
        public string title;
        [Tooltip("Titles by language keys")]
        public LanguageData[] titles;
        [Tooltip("Default description")]
        [TextArea]
        public string description;
        [Tooltip("Descriptions by language keys")]
        public LanguageData[] descriptions;
        public string category;
        public Sprite icon;
        
        public virtual string Id { get { return name; } }
        public virtual string Title
        {
            get { return Language.GetText(titles, title); }
        }
        public virtual string Description
        {
            get { return Language.GetText(descriptions, description); }
        }
        public int DataId { get { return MakeDataId(Id); } }
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }

        public static int MakeDataId(string id)
        {
            return id.GenerateHashId();
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (Validate())
                EditorUtility.SetDirty(this);
        }
#endif

        public virtual bool Validate()
        {
            return false;
        }

        public virtual void PrepareRelatesData()
        {

        }
    }
}
