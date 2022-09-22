using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace MultiplayerARPG
{
    public class BuildSettingMenu
    {
        [MenuItem(EditorMenuConsts.BUILD_SETUP_OFFLINE_LAN_MENU, false, EditorMenuConsts.BUILD_SETUP_OFFLINE_LAN_ORDER)]
        public static void BuildSetupOfflineLan()
        {
            AddToDefines("UNITY_SERVER");
        }

        [MenuItem(EditorMenuConsts.BUILD_SETUP_MMO_MENU, false, EditorMenuConsts.BUILD_SETUP_MMO_ORDER)]
        public static void BuildSetupMMO()
        {
            RemoveFromDefines("UNITY_SERVER");
        }

        [MenuItem(EditorMenuConsts.BUILD_SETUP_MMO_SERVER_INCLUDE_MENU, false, EditorMenuConsts.BUILD_SETUP_MMO_SERVER_INCLUDE_ORDER)]
        public static void BuildSetupMMOServerInclude()
        {
            AddToDefines("UNITY_SERVER");
        }

        private static void AddToDefines(string symbol)
        {
            string previousProjectDefines = GetCurrentProjectDefines(out BuildTargetGroup buildTargetGroup);
            List<string> projectDefines = previousProjectDefines.Split(';').ToList();
            if (!projectDefines.Contains(symbol))
                projectDefines.Add(symbol);
            string newDefines = string.Join(";", projectDefines.ToArray());
            if (previousProjectDefines != newDefines)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
        }

        public static void RemoveFromDefines(string symbol)
        {
            string previousProjectDefines = GetCurrentProjectDefines(out BuildTargetGroup buildTargetGroup);
            List<string> projectDefines = previousProjectDefines.Split(';').ToList();
            if (projectDefines.Contains(symbol))
                projectDefines.Remove(symbol);
            string newDefines = string.Join(";", projectDefines.ToArray());
            if (previousProjectDefines != newDefines)
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
        }

        private static string GetCurrentProjectDefines(out BuildTargetGroup buildTargetGroup)
        {
            buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (buildTargetGroup == BuildTargetGroup.Unknown)
            {
                PropertyInfo propertyInfo = typeof(EditorUserBuildSettings).GetProperty("activeBuildTargetGroup", BindingFlags.Static | BindingFlags.NonPublic);
                if (propertyInfo != null)
                    buildTargetGroup = (BuildTargetGroup)propertyInfo.GetValue(null, null);
            }
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        }
    }
}
