using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract partial class BaseGameData : ScriptableObject, IGameData, IPatchableData, IComparable
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
            set { defaultTitle = value; }
        }
        public LanguageData[] LanguageSpecificTitles
        {
            get { return languageSpecificTitles; }
            set { languageSpecificTitles = value; }
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
            set { defaultDescription = value; }
        }
        public LanguageData[] LanguageSpecificDescriptions
        {
            get { return languageSpecificDescriptions; }
            set { languageSpecificDescriptions = value; }
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
            set { category = value; }
        }

#if UNITY_EDITOR || !UNITY_SERVER
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [SerializeField]
        [AddressableAssetConversion(nameof(addressableIcon))]
        [PreviewSprite(50)]
        protected Sprite icon;
#endif
        public Sprite Icon
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return icon;
#else
                return null;
#endif
            }
            set
            {
#if !EXCLUDE_PREFAB_REFS
                icon = value;
#endif
            }
        }
        
        [SerializeField]
        protected AssetReferenceSprite addressableIcon;
        public AssetReferenceSprite AddressableIcon
        {
            get
            {
                return addressableIcon;
            }
            set
            {
                addressableIcon = value;
            }
        }

        public async UniTask<Sprite> GetIcon()
        {
#if !EXCLUDE_PREFAB_REFS
            if (icon != null)
                return icon;
#endif
            if (addressableIcon.IsDataValid())
                return await addressableIcon.LoadAssetAsync<Sprite>();
            return null;
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

        [NonSerialized]
        protected int? hashCode;
        public int HashCode
        {
            get
            {
                if (!hashCode.HasValue)
                    hashCode = $"{GetType().FullName}_{Id}".GetHashCode();
                return hashCode.Value;
            }
        }

        public readonly static Dictionary<int, string> IdMap = new Dictionary<int, string>();
        public readonly static Dictionary<string, int> DataIdMap = new Dictionary<string, int>();

        public static int MakeDataId(string id)
        {
            if (DataIdMap.TryGetValue(id, out int dataId))
                return dataId;
            dataId = id.GenerateHashId();
            IdMap[dataId] = id;
            DataIdMap[id] = dataId;
            return dataId;
        }

        protected virtual void OnEnable()
        {
            string key = this.GetPatchKey();
            if (PatchDataManager.PatchableData.TryAdd(key, this) &&
                PatchDataManager.PatchingData.TryGetValue(key, out Dictionary<string, object> patch))
            {
                this.ApplyPatch(patch);
            }
        }

        protected virtual void OnDisable()
        {
            string key = this.GetPatchKey();
            PatchDataManager.PatchableData.Remove(key);
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

        public override int GetHashCode()
        {
            return HashCode;
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        [ContextMenu("Test Export Data")]
        public void TestExportData()
        {
            var data = this.GetExportData();
            var json = JsonConvert.SerializeObject(data);
            Debug.Log($"Exporting data is:\n{json}");
        }
    }
}