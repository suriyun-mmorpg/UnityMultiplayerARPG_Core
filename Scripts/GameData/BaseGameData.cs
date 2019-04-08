using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameData : ScriptableObject
    {
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
            get
            {
                if (CacheTitles.ContainsKey(LanguageManager.CurrentLanguageKey))
                    return CacheTitles[LanguageManager.CurrentLanguageKey];
                return title;
            }
        }
        public virtual string Description
        {
            get
            {
                if (CacheDescriptions.ContainsKey(LanguageManager.CurrentLanguageKey))
                    return CacheDescriptions[LanguageManager.CurrentLanguageKey];
                return description;
            }
        }
        public int DataId { get { return Id.GenerateHashId(); } }
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }

        private Dictionary<string, string> cacheTitles;
        public Dictionary<string, string> CacheTitles
        {
            get
            {
                if (cacheTitles == null)
                {
                    cacheTitles = new Dictionary<string, string>();
                    if (titles != null)
                    {
                        foreach (LanguageData entry in titles)
                        {
                            cacheTitles[entry.key] = entry.value;
                        }
                    }
                }
                return cacheTitles;
            }
        }

        private Dictionary<string, string> cacheDescriptions;
        public Dictionary<string, string> CacheDescriptions
        {
            get
            {
                if (cacheDescriptions == null)
                {
                    cacheDescriptions = new Dictionary<string, string>();
                    if (descriptions != null)
                    {
                        foreach (LanguageData entry in descriptions)
                        {
                            cacheDescriptions[entry.key] = entry.value;
                        }
                    }
                }
                return cacheDescriptions;
            }
        }
    }
}
