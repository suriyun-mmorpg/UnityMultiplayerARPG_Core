using UnityEngine;
using UnityEngine.Serialization;
using System;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract partial class BaseGameData : ScriptableObject, IGameData, IComparable
    {
        [Tooltip("Game data ID, if this is empty it will uses file's name as ID")]
        [SerializeField]
        protected string id = string.Empty;
        public virtual string Id
        {
            get { return string.IsNullOrEmpty(id) ? name : id; }
            set { id = value; }
        }

        [Category("Generic Settings")]
        [SerializeField]
        [FormerlySerializedAs("title")]
        protected string defaultTitle = string.Empty;
        [SerializeField]
        [FormerlySerializedAs("titles")]
        protected LanguageData[] languageSpecificTitles;
        public string DefaultTitle
        {
            get { return defaultTitle; }
        }
        public LanguageData[] LanguageSpecificTitles
        {
            get { return languageSpecificTitles; }
        }
        [JsonIgnore]
        public virtual string Title
        {
            get { return Language.GetText(languageSpecificTitles, defaultTitle); }
        }

        [SerializeField]
        [FormerlySerializedAs("description")]
        [TextArea]
        protected string defaultDescription = string.Empty;
        [SerializeField]
        [FormerlySerializedAs("descriptions")]
        protected LanguageData[] languageSpecificDescriptions;
        public string DefaultDescription
        {
            get { return defaultDescription; }
        }
        public LanguageData[] LanguageSpecificDescriptions
        {
            get { return languageSpecificDescriptions; }
        }
        [JsonIgnore]
        public virtual string Description
        {
            get { return Language.GetText(languageSpecificDescriptions, defaultDescription); }
        }

        [SerializeField]
        protected string category = string.Empty;
        public string Category
        {
            get { return category; }
        }

        [SerializeField]
        [PreviewSprite(50)]
        protected Sprite icon;
        public Sprite Icon
        {
            get { return icon; }
        }


        [NonSerialized]
        protected int? dataId;
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
            this.InvokeInstanceDevExtMethods("PrepareRelatesData");
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

        public override string ToString()
        {
            return Id;
        }
    }
}
