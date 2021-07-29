using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract partial class BaseGameData : ScriptableObject, IGameData, IComparable
    {
        [Tooltip("Game data ID, if this is empty it will uses file's name as ID")]
        public string id;
        [Category("Generic Settings")]
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
        [PreviewSprite(50)]
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

        [NonSerialized]
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

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            BaseGameData otherGameData = obj as BaseGameData;
            if (otherGameData != null)
                return Id.CompareTo(otherGameData.Id);
            else
                throw new ArgumentException("Object is not a BaseGameData");
        }
    }
}
