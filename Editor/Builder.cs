using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Pipeline.Utilities;

namespace MultiplayerARPG
{
    public static class Builder
    {
        [MenuItem(EditorMenuConsts.PREPARE_ADDRESSABLE_ASSETS_MENU, false, EditorMenuConsts.PREPARE_ADDRESSABLE_ASSETS_ORDER)]
        public static void PrepareAddressableAssets()
        {
            // Delete all from both server and client
            foreach (var group in EditorGlobalData.SettingsInstance.serverAddressableGroups)
            {
                AddressableAssetSettingsDefaultObject.Settings.groups.Remove(group);
            }
            foreach (var group in EditorGlobalData.SettingsInstance.clientAddressableGroups)
            {
                AddressableAssetSettingsDefaultObject.Settings.groups.Remove(group);
            }
#if UNITY_SERVER
            string profileName = EditorGlobalData.SettingsInstance.serverBuildProfileName;
            if (!string.IsNullOrWhiteSpace(profileName) &&
                AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetAllProfileNames().Contains(profileName))
                AddressableAssetSettingsDefaultObject.Settings.activeProfileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId(profileName);
            foreach (var group in EditorGlobalData.SettingsInstance.serverAddressableGroups)
            {
                AddressableAssetSettingsDefaultObject.Settings.groups.Add(group);
            }
#else
            string profileName = EditorGlobalData.SettingsInstance.clientBuildProfileName;
            if (!string.IsNullOrWhiteSpace(profileName) &&
                AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetAllProfileNames().Contains(profileName))
                AddressableAssetSettingsDefaultObject.Settings.activeProfileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId(profileName);
            foreach (var group in EditorGlobalData.SettingsInstance.clientAddressableGroups)
            {
                AddressableAssetSettingsDefaultObject.Settings.groups.Add(group);
            }
#endif
        }

        public static void BuildWindows64Server()
        {
            string outputPath = string.Empty;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].Equals("-outputPath") && i + 1 < args.Length)
                    outputPath = args[i + 1];
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                UnityEngine.Debug.LogError("No output path");
                return;
            }

            List<string> scenes = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                {
                    scenes.Add(EditorBuildSettings.scenes[i].path);
                    UnityEngine.Debug.Log($"Add {EditorBuildSettings.scenes[i].path} to scenes in build list.");
                }
            }
#if UNITY_2021_1_OR_NEWER
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                locationPathName = outputPath,
                scenes = scenes.ToArray(),
            };
#else
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
                locationPathName = outputPath,
                scenes = scenes.ToArray(),
            };
#endif
            PrepareAddressableAssets();
            AddressableAssetSettingsDefaultObject.Settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
            AddressableAssetSettings.CleanPlayerContent();
            BuildCache.PurgeCache(false);
            AddressableAssetSettings.BuildPlayerContent();
            BuildPipeline.BuildPlayer(options);
        }

        public static void BuildLinux64Server()
        {
            string outputPath = string.Empty;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].Equals("-outputPath") && i + 1 < args.Length)
                    outputPath = args[i + 1];
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                UnityEngine.Debug.LogError("No output path");
                return;
            }

            List<string> scenes = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                {
                    scenes.Add(EditorBuildSettings.scenes[i].path);
                    UnityEngine.Debug.Log($"Add {EditorBuildSettings.scenes[i].path} to scenes in build list.");
                }
            }
#if UNITY_2021_1_OR_NEWER
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                locationPathName = outputPath,
                scenes = scenes.ToArray(),
            };
#else
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None,
                locationPathName = outputPath,
                scenes = scenes.ToArray(),
            };
#endif
            PrepareAddressableAssets();
            AddressableAssetSettingsDefaultObject.Settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
            AddressableAssetSettings.CleanPlayerContent();
            BuildCache.PurgeCache(false);
            AddressableAssetSettings.BuildPlayerContent();
            BuildPipeline.BuildPlayer(options);
        }
    }
}
