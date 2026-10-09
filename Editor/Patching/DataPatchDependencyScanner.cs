using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerARPG
{
    public static class DataPatchDependencyScanner
    {
        public static DataPatchRegistry Scan(DataPatchProfile profile, DataPatchBuildLog log = null)
        {
            if (profile == null || profile.database == null)
                throw new InvalidOperationException("Select a game database on the patch profile.");
            bool ownsLog = log == null;
            log = log ?? new DataPatchBuildLog(profile);
            var registry = new DataPatchRegistry(false, (error, value) => log.Error("Discover data (skipped)", value as Object, error));
            registry.AddGraph(profile.database);
            var pending = new Queue<Object>();
            var visited = new HashSet<int>();
            var paths = new HashSet<string>(StringComparer.Ordinal);
            pending.Enqueue(profile.database);
            foreach (Object root in profile.additionalRoots ?? Array.Empty<Object>())
                pending.Enqueue(root);
            while (pending.Count > 0)
            {
                Object asset = pending.Dequeue();
                if (asset == null || asset == profile || !visited.Add(asset.GetInstanceID()))
                    continue;
                if (asset is IPatchableData)
                    registry.AddGraph(asset);
                try
                {
                    string path = AssetDatabase.GetAssetPath(asset);
                    if (!string.IsNullOrEmpty(path) && paths.Add(path))
                        foreach (string dependency in AssetDatabase.GetDependencies(path, true))
                        {
                            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(dependency);
                            if (assetType == null || !typeof(GameObject).IsAssignableFrom(assetType) && !typeof(ScriptableObject).IsAssignableFrom(assetType))
                                continue;
                            foreach (Object child in AssetDatabase.LoadAllAssetsAtPath(dependency))
                                pending.Enqueue(child);
                        }

                    if (asset is GameObject prefab)
                        foreach (Component component in prefab.GetComponentsInChildren<Component>(true))
                            pending.Enqueue(component);
                    if (!(asset is GameObject) && !(asset is Component) && !(asset is ScriptableObject))
                        continue;
                    using (var serialized = new SerializedObject(asset))
                    {
                        var property = serialized.GetIterator();
                        while (property.Next(true))
                        {
                            if (property.propertyType == SerializedPropertyType.ObjectReference)
                                pending.Enqueue(property.objectReferenceValue);
                            // Addressables serialize their target GUID instead of a Unity object reference.
                            if (property.propertyType == SerializedPropertyType.String && property.name == "m_AssetGUID" && !string.IsNullOrEmpty(property.stringValue))
                            {
                                string targetPath = AssetDatabase.GUIDToAssetPath(property.stringValue);
                                if (string.IsNullOrEmpty(targetPath))
                                {
                                    log.Error("Missing Addressable (skipped)", asset, new InvalidOperationException($"Unresolved Addressable GUID {property.stringValue} in {path} ({property.propertyPath})."));
                                    continue;
                                }

                                pending.Enqueue(AssetDatabase.LoadMainAssetAtPath(targetPath));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Scan asset (remaining fields skipped)", asset, ex);
                }
            }

            // Keep discovered assets even when other dependencies could not be scanned.
            Undo.RecordObject(profile, "Scan patch dependencies");
            profile.scannedData = registry.Targets.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Value).ToArray();
            EditorUtility.SetDirty(profile);
            if (ownsLog)
                log.Save($"Discovered {registry.Targets.Count} assets. Missing/conflicting dependencies were skipped.");
            return registry;
        }
    }
}
