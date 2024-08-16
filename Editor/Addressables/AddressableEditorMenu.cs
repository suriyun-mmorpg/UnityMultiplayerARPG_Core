using Insthync.AddressableAssetTools;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    public static class AddressableEditorMenu
    {
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

        public static readonly HashSet<string> findUnconvertibleAssemblies = new HashSet<string>() { "Assembly-CSharp" };
        [MenuItem(EditorMenuConsts.FIND_UNCONVERTIBLE_OBJECTS_MENU, false, EditorMenuConsts.FIND_UNCONVERTIBLE_OBJECTS_ORDER)]
        public static void FindUnconvertibleObjects()
        {
            foreach (string assemblyName in findUnconvertibleAssemblies)
            {
                LogUnconvertibleTypesFromAssembly(assemblyName);
            }
        }

        static void LogUnconvertibleTypesFromAssembly(string name)
        {
            Debug.Log("--- Log Start ---");
            HashSet<object> loggedTypes = new HashSet<object>();
            try
            {
                // Get project assembly
                Assembly asm = Assembly.Load(new AssemblyName(name));

                // Filter out all that unconvertible
                foreach (System.Type type in asm.GetTypes())
                {
                    if (!type.IsSubclassOf(typeof(BaseGameData)))
                        continue;
                    if (type.HasAttribute<NotPatchableAttribute>())
                        continue;
                    LogUnconvertibleFields(loggedTypes, type.FullName, type);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            Debug.Log("--- Log End ---");
        }

        static void LogUnconvertibleFields(HashSet<object> loggedTypes, string treeLog, System.Type objectType)
        {
            if (loggedTypes.Contains(objectType))
                return;
            loggedTypes.Add(objectType);
            List<FieldInfo> fields = new List<FieldInfo>(objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            for (int i = fields.Count - 1; i >= 0; --i)
            {
                bool isRemoving = false;
                if (!fields[i].IsPublic && !fields[i].HasAttribute<SerializeField>())
                {
                    isRemoving = true;
                }
                if (fields[i].HasAttribute<System.NonSerializedAttribute>())
                {
                    isRemoving = true;
                }
                if (fields[i].HasAttribute<AddressableAssetConversionAttribute>() || fields[i].HasAttribute<NotPatchableAttribute>())
                {
                    isRemoving = true;
                }
                if (isRemoving)
                {
                    fields.RemoveAt(i);
                }
            }
            foreach (FieldInfo field in fields)
            {
                System.Type fieldType = field.FieldType;
                if (fieldType.IsListOrArray(out System.Type elementType))
                {
                    string nodeName = $"{field.Name}({elementType.FullName})";
                    if (elementType.IsSubclassOf(typeof(System.Delegate)))
                    {
                        continue;
                    }
                    if (elementType.IsSubclassOf(typeof(AssetReference)))
                    {
                        continue;
                    }
                    if (elementType.IsSubclassOf(typeof(Object)) && !elementType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        Debug.Log($"{treeLog} -> {nodeName} is object which is not convertible");
                        continue;
                    }
                    LogUnconvertibleFields(loggedTypes, $"{treeLog} -> {nodeName}", elementType);
                }
                else if (fieldType.IsClass)
                {
                    string nodeName = $"{field.Name}({fieldType.FullName})";
                    if (fieldType.IsSubclassOf(typeof(System.Delegate)))
                    {
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(AssetReference)))
                    {
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(Object)) && !fieldType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        Debug.Log($"{treeLog} -> {nodeName} is object which is not convertible");
                        continue;
                    }
                    LogUnconvertibleFields(loggedTypes, $"{treeLog} -> {nodeName}", fieldType);
                }
                else if (fieldType.IsValueType && !fieldType.IsPrimitive && !fieldType.IsEnum)
                {
                    string nodeName = $"{field.Name}({fieldType.FullName})";
                    LogUnconvertibleFields(loggedTypes, $"{treeLog} -> {nodeName}", fieldType);
                }
            }
        }
    }
}