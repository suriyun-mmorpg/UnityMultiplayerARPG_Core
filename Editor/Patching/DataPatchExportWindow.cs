using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public sealed class DataPatchExportWindow : EditorWindow
    {
        private DataPatchProfile database;
        private DataPatchRegistry registry;
        private DataPatchBuildLog log;
        private ScriptableObject[] assets = Array.Empty<ScriptableObject>();
        private string[] recordLabels = Array.Empty<string>();
        private string[] dataTypes = new[] { "All types" };
        private readonly List<int> matches = new List<int>();
        private Dictionary<string, DataPatchEntry> reviewedEntries;
        private DataPatchEntry selectedEntry;
        private string result = "";
        private string search = "";
        private string typeFilter = "All types";
        private int selected = -1;
        private int page;
        private Vector2 scroll;
        private Vector2 recordScroll;
        private bool showJson;
        private readonly HashSet<string> expanded = new HashSet<string>();
        private int jsonRecord = -1;
        private string recordJson = "";
        private string jsonSource;
        private DataPatchJsonText jsonText;
        private GUIStyle jsonStyle;
        private Font jsonFont;

        public static void Open(DataPatchProfile profile, DataPatchEntry[] snapshot = null)
        {
            var window = GetWindow<DataPatchExportWindow>(true, "Preview Patch Data");
            window.minSize = new Vector2(650, 550);
            window.database = profile;
            window.reviewedEntries = snapshot?.ToDictionary(entry => DataPatchRegistry.Key(entry.dataType, entry.dataId));
            window.BuildCatalog();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            database = (DataPatchProfile)EditorGUILayout.ObjectField("Patch Profile", database, typeof(DataPatchProfile), false);
            if (EditorGUI.EndChangeCheck())
            {
                reviewedEntries = null;
                BuildCatalog();
            }
            using (new EditorGUI.DisabledScope(database == null || EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Refresh Source Preview"))
                {
                    reviewedEntries = null;
                    BuildCatalog();
                }
                if (GUILayout.Button("Open Upload Dialog"))
                    DataPatchUploadWindow.Open(database);
            }
            if (!string.IsNullOrEmpty(result))
                EditorGUILayout.HelpBox(result, MessageType.Info);
            using (new EditorGUI.DisabledScope(!DataPatchBuildLog.HasLog(database)))
                if (GUILayout.Button("Show Log"))
                    DataPatchBuildLog.Show(database);
            EditorGUILayout.HelpBox(reviewedEntries == null ?
                "Select a record to generate its preview. Use the profile's Scan button after changing asset dependencies." :
                "This is the prepared upload snapshot. Refresh Source Preview to inspect later asset edits.", MessageType.Info);
            DrawReview();
        }

        private void BuildCatalog()
        {
            selected = -1;
            selectedEntry = null;
            jsonRecord = -1;
            jsonSource = null;
            recordJson = "";
            expanded.Clear();
            scroll = Vector2.zero;
            page = 0;
            try
            {
                log = new DataPatchBuildLog(database);
                registry = DataPatchSnapshotBuilder.CreateRegistry(database, log);
                assets = registry.Targets.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Where(pair => reviewedEntries == null || reviewedEntries.ContainsKey(pair.Key))
                    .Select(pair => pair.Value).ToArray();
                recordLabels = assets.Select(asset => $"{asset.GetType().Name} / {DataPatchRegistry.DataId(asset)} / {asset.name}").ToArray();
                dataTypes = new[] { "All types" }.Concat(assets.Select(asset => asset.GetType().Name).Distinct().OrderBy(type => type, StringComparer.Ordinal)).ToArray();
                if (!dataTypes.Contains(typeFilter))
                    typeFilter = "All types";
                result = $"{assets.Length} available records. JSON and validation are generated only for the selected record.";
                if (log.ErrorCount > 0)
                    log.Save(result);
            }
            catch (Exception ex)
            {
                assets = Array.Empty<ScriptableObject>();
                recordLabels = Array.Empty<string>();
                result = ex.Message;
            }
            FilterRecords();
            Repaint();
        }

        private void FilterRecords()
        {
            matches.Clear();
            for (int i = 0; i < assets.Length; ++i)
            {
                if (typeFilter != "All types" && assets[i].GetType().Name != typeFilter)
                    continue;
                if (!string.IsNullOrEmpty(search) && recordLabels[i].IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                matches.Add(i);
            }
            page = 0;
        }

        private void SelectRecord(int index)
        {
            selected = index;
            selectedEntry = null;
            jsonRecord = -1;
            expanded.Clear();
            scroll = Vector2.zero;
            ScriptableObject asset = assets[index];
            string key = DataPatchRegistry.Key(asset.GetType().Name, DataPatchRegistry.DataId(asset));
            if (reviewedEntries != null && reviewedEntries.TryGetValue(key, out selectedEntry))
                return;
            try
            {
                selectedEntry = DataPatchSnapshotBuilder.ExportRecord(asset, registry, log);
                log.Save($"Previewed {key}. Validation errors are advisory; runtime validation still applies.");
                result = $"Previewed {key}; {log.ErrorCount} errors collected. Use Show Log for details.";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
        }

        private void DrawReview()
        {
            EditorGUI.BeginChangeCheck();
            search = EditorGUILayout.TextField("Search name / ID", search);
            typeFilter = dataTypes[EditorGUILayout.Popup("Data Type", Math.Max(0, Array.IndexOf(dataTypes, typeFilter)), dataTypes)];
            if (EditorGUI.EndChangeCheck())
                FilterRecords();
            recordScroll = EditorGUILayout.BeginScrollView(recordScroll, GUILayout.Height(160));
            int end = Math.Min(matches.Count, (page + 1) * 50);
            for (int i = page * 50; i < end; ++i)
            {
                int index = matches[i];
                if (GUILayout.Toggle(selected == index, recordLabels[index], "Button") && selected != index)
                    SelectRecord(index);
            }
            if (matches.Count == 0)
                EditorGUILayout.LabelField("No matching records.");
            EditorGUILayout.EndScrollView();
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(page == 0))
                    if (GUILayout.Button("Previous"))
                        --page;
                EditorGUILayout.LabelField($"Page {page + 1} / {Math.Max(1, (matches.Count + 49) / 50)} - {matches.Count} matches");
                using (new EditorGUI.DisabledScope((page + 1) * 50 >= matches.Count))
                    if (GUILayout.Button("Next"))
                        ++page;
            }
            if (selectedEntry == null)
            {
                EditorGUILayout.LabelField("Select a record to preview its data.");
                return;
            }
            showJson = EditorGUILayout.Toggle("Show JSON", showJson);
            if (showJson)
            {
                DrawJsonReview();
                return;
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.ObjectField("Source Asset", assets[selected], typeof(ScriptableObject), false);
            foreach (JProperty field in selectedEntry.data.Properties())
                DrawValue(field.Name, field.Value, field.Name);
            EditorGUILayout.EndScrollView();
        }

        private void DrawJsonReview()
        {
            if (jsonRecord != selected)
            {
                jsonRecord = selected;
                recordJson = selectedEntry != null ? JsonConvert.SerializeObject(selectedEntry, Formatting.Indented) : "";
            }

            string source = recordJson;
            if (!ReferenceEquals(jsonSource, source))
            {
                jsonSource = source;
                jsonText = new DataPatchJsonText(source);
                scroll = Vector2.zero;
            }

            if (GUILayout.Button("Copy record JSON"))
                EditorGUIUtility.systemCopyBuffer = source;
            EditorGUILayout.LabelField($"{jsonText.LineCount:N0} lines - scroll to view the complete JSON");
            if (jsonStyle == null)
            {
                jsonFont = Font.CreateDynamicFontFromOSFont(new[]{"Consolas", "Menlo", "Liberation Mono"}, 12);
                jsonStyle = new GUIStyle(EditorStyles.label)
                {font = jsonFont, fontSize = 12, wordWrap = false, padding = new RectOffset(), margin = new RectOffset()};
            }

            Rect viewport = GUILayoutUtility.GetRect(100, 10000, 120, 10000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            float lineHeight = Math.Max(16, jsonStyle.lineHeight);
            float charWidth = Math.Max(1, jsonStyle.CalcSize(new GUIContent("M")).x);
            Rect content = new Rect(0, 0, Math.Max(viewport.width - 18, jsonText.MaxColumns * charWidth + 8), Math.Max(viewport.height - 18, jsonText.LineCount * lineHeight + 8));
            scroll = GUI.BeginScrollView(viewport, scroll, content);
            try
            {
                int firstLine = Math.Max(0, (int)(scroll.y / lineHeight));
                int lastLine = Math.Min(jsonText.LineCount, firstLine + (int)(viewport.height / lineHeight) + 2);
                int firstColumn = Math.Max(0, (int)(scroll.x / charWidth));
                int columns = Math.Max(1, (int)(viewport.width / charWidth) + 2);
                for (int line = firstLine; line < lastLine; ++line)
                    GUI.Label(new Rect(firstColumn * charWidth, line * lineHeight, viewport.width + charWidth * 2, lineHeight), jsonText.Slice(line, firstColumn, columns), jsonStyle);
            }
            finally
            {
                GUI.EndScrollView();
            }
        }

        private void OnDisable()
        {
            if (jsonFont != null)
                DestroyImmediate(jsonFont);
            jsonFont = null;
            jsonStyle = null;
        }

        private void DrawValue(string label, JToken value, string path)
        {
            if (value is JObject || value is JArray)
            {
                string heading = value is JArray array ? $"{label} [{array.Count}]" : label;
                bool open = EditorGUILayout.Foldout(expanded.Contains(path), heading, true);
                if (open)
                    expanded.Add(path);
                else
                    expanded.Remove(path);
                if (!open)
                    return;
                ++EditorGUI.indentLevel;
                if (value is JObject fields)
                    foreach (JProperty field in fields.Properties())
                        DrawValue(field.Name, field.Value, path + "/" + field.Name);
                else
                    for (int i = 0; i < ((JArray)value).Count; ++i)
                        DrawValue($"[{i}]", value[i], path + "/" + i);
                --EditorGUI.indentLevel;
                return;
            }

            EditorGUILayout.LabelField(label, value.Type == JTokenType.Null ? "null" : value.ToString());
        }

    }
}
