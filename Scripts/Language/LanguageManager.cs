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
        public static readonly Dictionary<string, Dictionary<string, string>> Languages = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, string> texts = new Dictionary<string, string>();
        public static Dictionary<string, string> Texts { get { return texts; } }
        private static string currentLanguageKey = string.Empty;
        public static string CurrentLanguageKey { get { return currentLanguageKey; } }
        private static string currentPlayerPrefsKey = string.Empty;

        [Header("Language Manager Configs")]
        public string defaultLanguageKey = "ENG";
        public string playerPrefsKey = "USER_LANG";
        public List<Language> languageList = new List<Language>();

        [Header("Add New Language")]
        [Tooltip("You can add new language by `Add New Language` context menu")]
        public string newLanguageKey;
        [InspectorButton("AddNewLanguage")]
        public bool addNewLanguage;

        private void Awake()
        {
            currentPlayerPrefsKey = playerPrefsKey;
            currentLanguageKey = PlayerPrefs.GetString(currentPlayerPrefsKey, defaultLanguageKey);
            Languages.Clear();
            Dictionary<string, string> tempNewData;
            foreach (Language language in languageList)
            {
                tempNewData = new Dictionary<string, string>();
                foreach (LanguageData data in language.dataList)
                {
                    if (tempNewData.ContainsKey(data.key))
                    {
                        Debug.LogWarning("[LanguageManager] Language " + language.languageKey + " already contains " + data.key);
                        continue;
                    }
                    tempNewData.Add(data.key, data.value);
                }
                Languages[language.languageKey] = tempNewData;
            }
            ChangeLanguage(currentLanguageKey);
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

        public static void ChangeLanguage(string languageKey)
        {
            if (!Languages.ContainsKey(languageKey))
                return;

            currentLanguageKey = languageKey;
            texts = Languages[languageKey];
            PlayerPrefs.SetString(currentPlayerPrefsKey, currentLanguageKey);
        }

        public static string GetText(string key, string defaultValue = "")
        {
            if (Texts.ContainsKey(key))
                return Texts[key];
            if (DefaultLocale.Texts.ContainsKey(key))
                return DefaultLocale.Texts[key];
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
