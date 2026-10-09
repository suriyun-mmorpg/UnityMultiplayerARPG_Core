using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public sealed class DataPatchBuildLog
    {
        private readonly DataPatchProfile profile;
        private readonly List<string> messages = new List<string>();
        public int ErrorCount { get; private set; }

        public DataPatchBuildLog(DataPatchProfile profile)
        {
            this.profile = profile;
            messages.Add($"Patch build: {DateTime.UtcNow:O}\nProfile: {AssetDatabase.GetAssetPath(profile)}");
        }

        public void Error(string stage, UnityEngine.Object asset, Exception error)
        {
            ++ErrorCount;
            messages.Add($"\n[{stage}] {AssetDatabase.GetAssetPath(asset)} ({(asset == null ? "no asset" : asset.name)})\n{error}");
        }

        public void Save(string summary)
        {
            string directory = Path.GetFullPath("Library/GameDataPatchLogs");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N") + ".log");
            File.WriteAllText(path, string.Join("\n", messages) + $"\n\n{summary}\nErrors: {ErrorCount}\n");
            EditorPrefs.SetString(Key(profile), path);
        }

        private static string Key(DataPatchProfile profile) => "DataPatchBuildLog:" + (profile == null ? "none" : (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(profile)) ? Application.dataPath + ":" + profile.GetInstanceID() : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(profile))));

        public static bool HasLog(DataPatchProfile profile) => File.Exists(EditorPrefs.GetString(Key(profile), ""));

        public static void Show(DataPatchProfile profile)
        {
            string path = EditorPrefs.GetString(Key(profile), "");
            if (File.Exists(path))
                EditorUtility.OpenWithDefaultApp(path);
        }
    }
}
