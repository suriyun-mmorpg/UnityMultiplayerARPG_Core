using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    public static class SceneGameDataCollector
    {
        [MenuItem("CONTEXT/DataPatchProfile/Add Patch Data From Active Scene")]
        private static void CollectProfileActive(MenuCommand command)
        {
            Collect((DataPatchProfile)command.context, false);
        }

        [MenuItem("CONTEXT/DataPatchProfile/Add Patch Data From Loaded Scenes")]
        private static void CollectProfileLoaded(MenuCommand command)
        {
            Collect((DataPatchProfile)command.context, true);
        }

        public static string Collect(DataPatchProfile profile, bool allLoadedScenes)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return "Collect scene patch data in Edit Mode.";
            var roots = new List<GameObject>();
            int count = allLoadedScenes ? SceneManager.sceneCount : 1;
            for (int i = 0; i < count; ++i)
            {
                Scene scene = allLoadedScenes ? SceneManager.GetSceneAt(i) : SceneManager.GetActiveScene();
                if (!scene.IsValid() || !scene.isLoaded || EditorSceneManager.IsPreviewScene(scene))
                    continue;
                roots.AddRange(scene.GetRootGameObjects());
            }

            return CollectFromRoots(profile, roots);
        }

        public static string CollectFromRoots(DataPatchProfile profile, IEnumerable<GameObject> roots)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            var scan = new Scan();
            foreach (GameObject root in roots)
                scan.VisitHierarchy(root);
            var registry = new DataPatchRegistry(false);
            registry.AddGraph(profile.database);
            foreach (ScriptableObject asset in profile.scannedData ?? Array.Empty<ScriptableObject>())
                registry.AddGraph(asset);
            foreach (UnityEngine.Object asset in profile.additionalRoots ?? Array.Empty<UnityEngine.Object>())
                registry.AddGraph(asset);
            foreach (ScriptableObject asset in scan.Data)
                registry.AddGraph(asset);

            // Store persistent asset roots so a later dependency scan retains scene-only data.
            var merged = new List<UnityEngine.Object>(profile.additionalRoots ?? Array.Empty<UnityEngine.Object>());
            int oldCount = merged.Count;
            foreach (ScriptableObject asset in scan.Data.OrderBy(AssetDatabase.GetAssetPath, StringComparer.Ordinal))
            {
                if (!merged.Contains(asset))
                    merged.Add(asset);
            }

            Undo.RecordObject(profile, "Add Scene Patch Data");
            profile.additionalRoots = merged.ToArray();
            profile.scannedData = registry.Targets.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Value).ToArray();
            EditorUtility.SetDirty(profile);
            string message = $"Added {merged.Count - oldCount} patch roots; catalog contains {profile.scannedData.Length} assets.";
            if (scan.Unsaved.Count > 0)
                message += $" Skipped {scan.Unsaved.Count} unsaved data objects.";
            if (scan.MissingGuids.Count > 0)
                message += $" Could not resolve {scan.MissingGuids.Count} Addressable asset GUIDs.";
            Debug.Log(message, profile);
            return message;
        }

        [MenuItem("CONTEXT/GameDatabase/Add Game Data From Active Scene")]
        private static void CollectActive(MenuCommand command) => Collect((GameDatabase)command.context, false);

        [MenuItem("CONTEXT/GameDatabase/Add Game Data From Loaded Scenes")]
        private static void CollectLoaded(MenuCommand command) => Collect((GameDatabase)command.context, true);

        public static string Collect(GameDatabase database, bool allLoadedScenes)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return "Collect scene game data in Edit Mode.";
            var roots = new List<GameObject>();
            int count = allLoadedScenes ? SceneManager.sceneCount : 1;
            for (int i = 0; i < count; ++i)
            {
                Scene scene = allLoadedScenes ? SceneManager.GetSceneAt(i) : SceneManager.GetActiveScene();
                if (!scene.IsValid() || !scene.isLoaded || EditorSceneManager.IsPreviewScene(scene))
                    continue;
                roots.AddRange(scene.GetRootGameObjects());
            }

            return CollectFromRoots(database, roots);
        }

        public static string CollectFromRoots(GameDatabase database, IEnumerable<GameObject> roots)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            var scan = new Scan();
            foreach (GameObject root in roots)
                scan.VisitHierarchy(root);
            // Validate the entire resulting data graph before changing any catalog.
            var registry = new DataPatchRegistry(false);
            registry.AddGraph(database);
            foreach (ScriptableObject asset in scan.Data)
                registry.AddGraph(asset);
            var changes = new Dictionary<FieldInfo, Array>();
            var counts = new List<string>();
            foreach (FieldInfo field in typeof(GameDatabase).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!field.FieldType.IsArray)
                    continue;
                Type element = field.FieldType.GetElementType();
                if (!typeof(IPatchableData).IsAssignableFrom(element))
                    continue;
                var existing = field.GetValue(database) as Array;
                var merged = existing == null ? new List<UnityEngine.Object>() : existing.Cast<UnityEngine.Object>().ToList();
                foreach (ScriptableObject asset in scan.Data.Where(element.IsInstanceOfType).OrderBy(AssetDatabase.GetAssetPath, StringComparer.Ordinal))
                    if (!merged.Contains(asset))
                        merged.Add(asset);
                int added = merged.Count - (existing?.Length ?? 0);
                if (added == 0)
                    continue;
                Array result = Array.CreateInstance(element, merged.Count);
                for (int i = 0; i < merged.Count; ++i)
                    result.SetValue(merged[i], i);
                changes.Add(field, result);
                counts.Add($"{field.Name}: +{added}");
            }

            if (changes.Count > 0)
            {
                Undo.RecordObject(database, "Add Scene Game Data");
                foreach (var change in changes)
                    change.Key.SetValue(database, change.Value);
                EditorUtility.SetDirty(database);
            }

            string message = counts.Count == 0 ? "No new game data to add." : "Added " + string.Join(", ", counts) + ".";
            message += $" Scanned {scan.PrefabCount} referenced prefabs.";
            if (scan.Unsaved.Count > 0)
                message += $" Skipped {scan.Unsaved.Count} unsaved data objects; save them as assets first.";
            if (scan.MissingGuids.Count > 0)
                message += $" Could not resolve {scan.MissingGuids.Count} Addressable asset GUIDs.";
            Debug.Log(message, database);
            return message;
        }

        private sealed class Scan
        {
            public readonly HashSet<ScriptableObject> Data = new HashSet<ScriptableObject>();
            public readonly HashSet<UnityEngine.Object> Unsaved = new HashSet<UnityEngine.Object>();
            public readonly HashSet<string> MissingGuids = new HashSet<string>();
            private readonly HashSet<GameObject> hierarchies = new HashSet<GameObject>();
            private readonly HashSet<UnityEngine.Object> inspected = new HashSet<UnityEngine.Object>();
            public int PrefabCount { get; private set; }

            public void VisitHierarchy(GameObject root)
            {
                if (root == null || !hierarchies.Add(root))
                    return;
                if (EditorUtility.IsPersistent(root))
                    ++PrefabCount;
                foreach (Component component in root.GetComponentsInChildren<Component>(true))
                    if (component != null)
                        Inspect(component);
            }

            private void VisitReference(UnityEngine.Object value)
            {
                if (value is ScriptableObject && value is IPatchableData)
                {
                    if (!EditorUtility.IsPersistent(value))
                    {
                        Unsaved.Add(value);
                        return;
                    }

                    if (Data.Add((ScriptableObject)value))
                        Inspect(value);
                }
                else if (value is GameObject prefab && EditorUtility.IsPersistent(prefab))
                    VisitHierarchy(prefab);
                else if (value is Component component && EditorUtility.IsPersistent(component))
                    VisitHierarchy(component.transform.root.gameObject);
            }

            private void Inspect(UnityEngine.Object value)
            {
                if (!inspected.Add(value))
                    return;
                using (var serialized = new SerializedObject(value))
                {
                    SerializedProperty property = serialized.GetIterator();
                    while (property.Next(true))
                    {
                        if (property.propertyType == SerializedPropertyType.ObjectReference)
                            VisitReference(property.objectReferenceValue);
                        // AssetReference GUIDs are serialized strings, not Unity object references.
                        if (property.propertyType == SerializedPropertyType.String && property.name == "m_AssetGUID" && !string.IsNullOrEmpty(property.stringValue))
                        {
                            string path = AssetDatabase.GUIDToAssetPath(property.stringValue);
                            if (string.IsNullOrEmpty(path))
                            {
                                MissingGuids.Add(property.stringValue);
                                continue;
                            }

                            if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                                VisitHierarchy(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                        }
                    }
                }
            }
        }
    }
}
