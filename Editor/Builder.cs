using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        [MenuItem(EditorMenuConsts.BAKE_SERVER_SCENE_MENU, false, EditorMenuConsts.BAKE_SERVER_SCENE_ORDER)]
        public static void BakeServerScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; ++i)
            {
                UnpackPrefabInstances(rootObjects[i]);
                Terrain[] terrains = rootObjects[i].GetComponentsInChildren<Terrain>();
                for (int j = 0; j < terrains.Length; ++j)
                {
                    Object.DestroyImmediate(terrains[j]);
                }
                TMPro.TextMeshPro[] tmps = rootObjects[i].GetComponentsInChildren<TMPro.TextMeshPro>();
                for (int j = 0; j < tmps.Length; ++j)
                {
                    Object.DestroyImmediate(tmps[j]);
                }
                Animator[] animators = rootObjects[i].GetComponentsInChildren<Animator>();
                for (int j = 0; j < animators.Length; ++j)
                {
                    Object.DestroyImmediate(animators[j]);
                }
                SkinnedMeshRenderer[] skinnedMeshRenderers = rootObjects[i].GetComponentsInChildren<SkinnedMeshRenderer>();
                for (int j = 0; j < skinnedMeshRenderers.Length; ++j)
                {
                    Object.DestroyImmediate(skinnedMeshRenderers[j]);
                }
                MeshRenderer[] meshRenderers = rootObjects[i].GetComponentsInChildren<MeshRenderer>();
                for (int j = 0; j < meshRenderers.Length; ++j)
                {
                    Object.DestroyImmediate(meshRenderers[j]);
                }
                MeshFilter[] meshFilters = rootObjects[i].GetComponentsInChildren<MeshFilter>();
                for (int j = 0; j < meshFilters.Length; ++j)
                {
                    Object.DestroyImmediate(meshFilters[j]);
                }
            }

            // Check if the scene is valid
            if (scene.IsValid())
            {
                // Show a save file dialog to choose the path
                string path = EditorUtility.SaveFilePanel("Save Scene As", "Assets", $"{scene.name}_SERVER", "unity");

                // Check if the path is valid
                if (!string.IsNullOrEmpty(path))
                {
                    // Convert the path to a relative path
                    string relativePath = FileUtil.GetProjectRelativePath(path);

                    // Save the scene to the new path
                    bool success = EditorSceneManager.SaveScene(scene, relativePath);

                    // Log the result
                    if (success)
                    {
                        Debug.Log("Scene saved successfully as " + relativePath);
                    }
                    else
                    {
                        Debug.LogError("Failed to save the scene.");
                    }
                }
                else
                {
                    Debug.LogError("Invalid path specified.");
                }
            }
            else
            {
                Debug.LogError("No valid active scene to save.");
            }
        }

        static void UnpackPrefabInstances(GameObject gameObject)
        {
            // Check if the GameObject is part of a prefab instance
            if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
            {
                // Unpack the prefab instance completely
                PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            // Recursively unpack child objects
            foreach (Transform child in gameObject.transform)
            {
                UnpackPrefabInstances(child.gameObject);
            }
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
