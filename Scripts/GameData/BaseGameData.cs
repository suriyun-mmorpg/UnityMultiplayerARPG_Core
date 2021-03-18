using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract partial class BaseGameData : ScriptableObject, IGameData
    {
        [Header("Game Data Configs")]
        [Tooltip("Game data ID, if this is empty it will uses file's name as ID")]
        public string id;
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
        
        public virtual string Id
        {
            get { return string.IsNullOrEmpty(id) ? name : id; }
        }
        public virtual string Title
        {
            get { return Language.GetText(titles, title); }
        }
        public virtual string Description
        {
            get { return Language.GetText(descriptions, description); }
        }

        private int? dataId;
        public int DataId
        {
            get
            {
                if (!dataId.HasValue)
                    dataId = MakeDataId(Id);
                return dataId.Value;
            }
        }

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

        [ContextMenu("Force Validate")]
        public virtual bool Validate()
        {
            return false;
        }

        public virtual void PrepareRelatesData()
        {

        }
    }
}
