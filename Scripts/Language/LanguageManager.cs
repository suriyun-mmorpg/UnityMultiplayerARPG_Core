using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class LanguageManager : MonoBehaviour
    {
        public static readonly Dictionary<string, string> Texts = new Dictionary<string, string>();
        public static string CurrentLanguageKey { get; private set; }
        public string defaultLanguageKey = "ENG";
        [Header("Add New Language")]
        [Tooltip("You can add new language by `Add New Language` context menu")]
        public string newLanguageKey;
        [InspectorButton("AddNewLanguage")]
        public bool addNewLanguage;
        [Header("Language List")]
        public List<Language> languageList = new List<Language>();
        public readonly Dictionary<string, Language> LanguageMap = new Dictionary<string, Language>();
        private void Awake()
        {
            SetupDefaultTexts();
            foreach (Language language in languageList)
            {
                LanguageMap[language.languageKey] = language;
            }
            ChangeLanguage(defaultLanguageKey);
        }

        private void SetupDefaultTexts()
        {
            Texts.Clear();
            foreach (KeyValuePair<string, string> pair in DefaultLocale.Texts)
            {
                Texts.Add(pair.Key, pair.Value);
            }
        }

        private void ChangeLanguage(string languageKey)
        {
            if (!LanguageMap.ContainsKey(languageKey))
                return;

            CurrentLanguageKey = languageKey;
            List<LanguageData> languageDataList = LanguageMap[languageKey].dataList;
            foreach (LanguageData data in languageDataList)
            {
                if (Texts.ContainsKey(data.key))
                    Texts[data.key] = data.value;
            }
        }

        public Language GetLanguageFromList(string languageKey)
        {
            foreach (Language language in languageList)
            {
                if (language.languageKey == languageKey)
                    return language;
            }
            return null;
        }

        [ContextMenu("Add New Language")]
        public void AddNewLanguage()
        {
            if (string.IsNullOrEmpty(newLanguageKey))
            {
                Debug.LogWarning("`New Language Key` is null or empty");
                return;
            }
            Language newLang = GetLanguageFromList(newLanguageKey);
            if (newLang == null)
            {
                newLang = new Language();
                newLang.languageKey = newLanguageKey;
                languageList.Add(newLang);
            }

            foreach (KeyValuePair<string, string> pair in DefaultLocale.Texts)
            {
                if (newLang.ContainKey(pair.Key))
                    continue;

                newLang.dataList.Add(new LanguageData()
                {
                    key = pair.Key,
                    value = pair.Value,
                });
            }
        }

        public static string GetText(string key, string defaultValue = "")
        {
            if (Texts.ContainsKey(key))
                return Texts[key];
            return defaultValue;
        }

        public static string GetUnknowTitle()
        {
            return GetText(GameMessage.Type.UnknowGameDataTitle.ToString(), "Unknow");
        }

        public static string GetUnknowDescription()
        {
            return GetText(GameMessage.Type.UnknowGameDataDescription.ToString(), "N/A");
        }
    }
}
