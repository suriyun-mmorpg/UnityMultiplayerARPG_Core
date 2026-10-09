using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MultiplayerARPG
{
    public sealed class DataPatchExportWindow : EditorWindow
    {
        private DataPatchProfile database;
        private string description = "";
        private string result = "";
        private bool uploading;
        private Vector2 scroll;
        private DataPatchEntry[] entries;
        private ScriptableObject[] assets;
        private string search = "";
        private string typeFilter = "All types";
        private int selected = -1;
        private Vector2 recordScroll;
        private bool showJson;
        private readonly HashSet<string> expanded = new HashSet<string>();
        private string reviewedScope;
        private string[] recordLabels;
        private string[] searchValues;
        private int page;
        private int jsonRecord = -1;
        private string recordJson = "";
        private string jsonSource;
        private DataPatchJsonText jsonText;
        private GUIStyle jsonStyle;
        private Font jsonFont;
        private string Scope => database == null ? "" : JsonConvert.SerializeObject(new
        {
        database.patchDatabaseId, database.dataPatchSettings.serviceUrl, database.dataPatchSettings.environment
        }

        );

        public static void Open(DataPatchProfile database)
        {
            var window = GetWindow<DataPatchExportWindow>(true, "Review Patch Data");
            if (window.uploading)
            {
                window.Focus();
                return;
            }

            window.minSize = new Vector2(650, 550);
            window.database = database;
            window.BuildPreview();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            using (new EditorGUI.DisabledScope(uploading))
                database = (DataPatchProfile)EditorGUILayout.ObjectField("Patch Profile", database, typeof(DataPatchProfile), false);
            if (EditorGUI.EndChangeCheck())
            {
                entries = null;
            }

            PatchUploadConfig config = PatchUploadConfig.LoadOrCreate();
            EditorGUILayout.LabelField("Target", database == null ? "Select a profile" : database.dataPatchSettings.environment + " / " + database.dataPatchSettings.serviceUrl);
            if (GUILayout.Button("Configure Upload"))
                Selection.activeObject = config;
            using (new EditorGUI.DisabledScope(uploading))
                description = EditorGUILayout.TextField("Description", description);
            using (new EditorGUI.DisabledScope(uploading || EditorApplication.isPlaying || database == null))
            {
                if (GUILayout.Button("Refresh Preview"))
                    BuildPreview();
                using (new EditorGUI.DisabledScope(entries == null || entries.Length == 0))
                    if (GUILayout.Button("Save All Data as ZIP (folders by type)"))
                        SaveZip();
                using (new EditorGUI.DisabledScope(entries == null || entries.Length == 0 || reviewedScope != Scope || string.IsNullOrWhiteSpace(database.patchDatabaseId) || string.IsNullOrWhiteSpace(config.secretKey) || (database == null || string.IsNullOrWhiteSpace(database.dataPatchSettings.serviceUrl))))
                    if (GUILayout.Button("Upload and Publish Reviewed Patch"))
                        Upload().Forget();
            }

            if (entries != null && reviewedScope != Scope)
                EditorGUILayout.HelpBox("The patch target changed. Refresh the preview before uploading.", MessageType.Warning);
            if (string.IsNullOrWhiteSpace(config.secretKey))
                EditorGUILayout.HelpBox("Configure the upload secret before creating a patch.", MessageType.Warning);
            if (!string.IsNullOrEmpty(result))
                EditorGUILayout.HelpBox(result, MessageType.Info);
            using (new EditorGUI.DisabledScope(!DataPatchBuildLog.HasLog(database)))
                if (GUILayout.Button("Show Log"))
                    DataPatchBuildLog.Show(database);
            DrawReview();
        }

        private void SaveZip()
        {
            string folder = EditorUtility.OpenFolderPanel("Choose folder for patch ZIP", "", "");
            if (string.IsNullOrEmpty(folder))
                return;
            try
            {
                string path = DataPatchZipExporter.SaveToFolder(folder, entries);
                result = $"Saved all {entries.Length} reviewed records to {path}";
                EditorUtility.RevealInFinder(path);
            }
            catch (Exception ex)
            {
                result = "Could not save patch ZIP: " + ex.Message;
            }
        }

        private void DrawReview()
        {
            if (entries == null)
                return;
            EditorGUILayout.LabelField($"{entries.Length} records");
            EditorGUILayout.HelpBox("Review the exported snapshot below. Upload sends these values; refresh to include later asset edits. Filtering only changes the view, not the uploaded records.", MessageType.Info);
            EditorGUI.BeginChangeCheck();
            search = EditorGUILayout.TextField("Search name / ID / field", search);
            string[] types = new[]{"All types"}.Concat(entries.Select(entry => entry.dataType).Distinct()).ToArray();
            typeFilter = types[EditorGUILayout.Popup("Data Type", Math.Max(0, Array.IndexOf(types, typeFilter)), types)];
            if (EditorGUI.EndChangeCheck())
                page = 0;
            recordScroll = EditorGUILayout.BeginScrollView(recordScroll, GUILayout.Height(160));
            int visible = 0;
            for (int i = 0; i < entries.Length; ++i)
            {
                DataPatchEntry entry = entries[i];
                if (typeFilter != "All types" && entry.dataType != typeFilter)
                    continue;
                string label = recordLabels[i];
                if (!string.IsNullOrEmpty(search) && searchValues[i].IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                ++visible;
                if (visible <= page * 50 || visible > (page + 1) * 50)
                    continue;
                if (GUILayout.Toggle(selected == i, label, "Button") && selected != i)
                {
                    selected = i;
                    expanded.Clear();
                    scroll = Vector2.zero;
                }
            }

            if (visible == 0)
                EditorGUILayout.LabelField("No matching records.");
            EditorGUILayout.EndScrollView();
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(page == 0))
                    if (GUILayout.Button("Previous"))
                        --page;
                EditorGUILayout.LabelField($"Page {page + 1} / {Math.Max(1, (visible + 49) / 50)} - {visible} matches");
                using (new EditorGUI.DisabledScope((page + 1) * 50 >= visible))
                    if (GUILayout.Button("Next"))
                        ++page;
            }

            showJson = EditorGUILayout.Toggle("Show JSON", showJson);
            if (showJson)
            {
                DrawJsonReview();
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (selected >= 0 && selected < entries.Length)
            {
                EditorGUILayout.ObjectField("Source Asset", assets[selected], typeof(ScriptableObject), false);
                foreach (JProperty field in entries[selected].data.Properties())
                    DrawValue(field.Name, field.Value, field.Name);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawJsonReview()
        {
            if (jsonRecord != selected)
            {
                jsonRecord = selected;
                recordJson = selected >= 0 && selected < entries.Length ? JsonConvert.SerializeObject(entries[selected], Formatting.Indented) : "";
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

        private void BuildPreview()
        {
            entries = null;
            try
            {
                if (database == null)
                    throw new InvalidOperationException("Select a patch profile.");
                var log = new DataPatchBuildLog(database);
                if (string.IsNullOrWhiteSpace(database.patchDatabaseId))
                    log.Error("Upload configuration", database, new InvalidOperationException("Database ID is empty. Preview is available; set an ID before upload."));
                var registry = DataPatchDependencyScanner.Scan(database, log);
                var collected = new List<DataPatchEntry>();
                foreach (var pair in registry.Targets.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                {
                    DataPatchEntry entry;
                    try
                    {
                        var asset = pair.Value;
                        entry = new DataPatchEntry{dataType = asset.GetType().Name, dataId = DataPatchRegistry.DataId(asset), data = DataPatchSerializer.Export(asset)};
                        if (entry.data.Count == 0)
                            continue;
                        collected.Add(entry);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Export record (omitted)", pair.Value, ex);
                        continue;
                    }

                    ScriptableObject copy = null;
                    try
                    {
                        copy = Instantiate(pair.Value);
                        DataPatchSerializer.ReadInto(copy, entry.data, registry);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Validate record (included in preview)", pair.Value, ex);
                    }
                    finally
                    {
                        if (copy != null)
                            DestroyImmediate(copy);
                    }
                }

                entries = collected.OrderBy(entry => entry.dataType, StringComparer.Ordinal).ThenBy(entry => entry.dataId).ToArray();
                log.Save($"Created {entries.Length} records. Validation errors are advisory during export; runtime validation still applies.");
                jsonRecord = -1;
                jsonSource = null;
                assets = entries.Select(entry => registry.Targets[DataPatchRegistry.Key(entry.dataType, entry.dataId)]).ToArray();
                recordLabels = entries.Select((entry, i) => $"{entry.dataType} / {entry.dataId} / {assets[i].name}").ToArray();
                searchValues = entries.Select((entry, i) => recordLabels[i] + " " + entry.data.ToString(Formatting.None)).ToArray();
                page = 0;
                reviewedScope = Scope;
                selected = entries.Length > 0 ? 0 : -1;
                expanded.Clear();
                result = $"Created {entries.Length} records; {log.ErrorCount} errors collected. Use Show Log for details.";
            }
            catch (Exception ex)
            {
                entries = null;
                result = ex.Message;
            }

            Repaint();
        }

        private async UniTaskVoid Upload()
        {
            uploading = true;
            try
            {
                if (entries == null || entries.Length == 0 || reviewedScope != Scope || string.IsNullOrWhiteSpace(database.patchDatabaseId))
                    throw new InvalidOperationException("Refresh and review the patch before uploading.");
                PatchUploadConfig config = PatchUploadConfig.LoadOrCreate();
                if (string.IsNullOrWhiteSpace(config.secretKey))
                    throw new InvalidOperationException("Upload secret is required.");
                if (!Uri.TryCreate(database.dataPatchSettings.serviceUrl, UriKind.Absolute, out Uri uri) || uri.Scheme != "https" && !uri.IsLoopback)
                    throw new InvalidOperationException("Use HTTPS, or loopback HTTP for local development.");
                string bodyHash = DataPatchHttp.Hash(JsonConvert.SerializeObject(new
                {
                description, entries
                }

                ));
                if (config.pendingPayloadHash != bodyHash || config.pendingDatabaseId != database.patchDatabaseId || config.pendingEnvironment != database.dataPatchSettings.environment || string.IsNullOrEmpty(config.pendingUploadId))
                    config.pendingUploadId = Guid.NewGuid().ToString("N");
                config.pendingPayloadHash = bodyHash;
                config.pendingDatabaseId = database.patchDatabaseId;
                config.pendingEnvironment = database.dataPatchSettings.environment;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                string id = config.pendingUploadId;
                string json = JsonConvert.SerializeObject(new
                {
                id, databaseId = database.patchDatabaseId, environment = database.dataPatchSettings.environment, schemaVersion = 1, description, entries
                }

                );
                string serviceUrl = database.dataPatchSettings.serviceUrl;
                string secret = config.secretKey;
                int timeout = database.dataPatchSettings.requestTimeoutSeconds;
                await Post(serviceUrl, secret, timeout, "/game-data-patches", json);
                string published = await Post(serviceUrl, secret, timeout, "/game-data-patches/" + id + "/publish", "{}");
                DataPatchRelease release = JsonConvert.DeserializeObject<DataPatchRelease>(published);
                DataPatchHttp.ValidateHash(release);
                result = $"Published release {release.id}, version {release.version}, {entries.Length} records.";
                config.pendingUploadId = "";
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {
                uploading = false;
                Repaint();
            }
        }

        private static async UniTask<string> Post(string serviceUrl, string secret, int timeout, string route, string json)
        {
            using (var request = new UnityWebRequest(serviceUrl.TrimEnd('/') + route, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = Math.Max(1, Math.Min(120, timeout));
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-api-key", secret);
                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException)
                {
                    throw new InvalidOperationException($"Patch upload failed (HTTP {request.responseCode}). Check endpoint and credential configuration; retry uses the same upload ID.");
                }

                if (request.result != UnityWebRequest.Result.Success)
                    throw new InvalidOperationException($"Patch upload failed (HTTP {request.responseCode}).");
                return request.downloadHandler.text;
            }
        }
    }
}
