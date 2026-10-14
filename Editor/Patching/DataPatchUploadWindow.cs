using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MultiplayerARPG
{
    public sealed class DataPatchUploadWindow : EditorWindow
    {
        private DataPatchProfile database;
        private DataPatchEntry[] entries;
        private string description = "";
        private string result = "";
        private string reviewedScope;
        private bool building;
        private bool partialPublishing = true;
        private string[] availableTypes = Array.Empty<string>();
        private readonly HashSet<string> selectedTypes = new HashSet<string>();
        private Vector2 typeScroll;
        private bool uploading;
        private int completed;
        private int total;
        private string[] typeSummaries = Array.Empty<string>();
        private Vector2 scroll;
        private CancellationTokenSource buildCancellation;
        private string Scope => database == null ? "" : JsonConvert.SerializeObject(new
        {
            database.patchDatabaseId,
            database.dataPatchSettings.serviceUrl,
            partialPublishing,
            types = partialPublishing ? selectedTypes.OrderBy(type => type, StringComparer.Ordinal).ToArray() : Array.Empty<string>(),
        });

        public static void Open(DataPatchProfile profile)
        {
            var window = GetWindow<DataPatchUploadWindow>(true, "Upload Patch Data");
            window.minSize = new Vector2(550, 620);
            if (!window.building && !window.uploading && window.database != profile)
            {
                window.database = profile;
                window.entries = null;
                window.typeSummaries = Array.Empty<string>();
                window.result = "";
            }
            if (!window.building && !window.uploading)
                window.LoadDataTypes();
            window.Focus();
        }

        private void OnGUI()
        {
            using (new EditorGUI.DisabledScope(building || uploading))
            {
                EditorGUI.BeginChangeCheck();
                database = (DataPatchProfile)EditorGUILayout.ObjectField("Patch Profile", database, typeof(DataPatchProfile), false);
                if (EditorGUI.EndChangeCheck())
                {
                    entries = null;
                    typeSummaries = Array.Empty<string>();
                    selectedTypes.Clear();
                    LoadDataTypes();
                }
                EditorGUILayout.LabelField("Target", database == null ? "Select a profile" : database.patchDatabaseId + " / " + database.dataPatchSettings.serviceUrl);
                description = EditorGUILayout.TextField("Description", description);
                partialPublishing = EditorGUILayout.Toggle("Publish partial updates", partialPublishing);
                using (new EditorGUI.DisabledScope(!partialPublishing))
                    DrawDataTypes();
                using (new EditorGUI.DisabledScope(database == null || EditorApplication.isPlayingOrWillChangePlaymode || partialPublishing && selectedTypes.Count == 0))
                    if (GUILayout.Button("Prepare Upload Snapshot"))
                        BuildSnapshot().Forget();
                using (new EditorGUI.DisabledScope(database == null))
                    if (GUILayout.Button("Open Data Preview"))
                        DataPatchExportWindow.Open(database, entries);
                using (new EditorGUI.DisabledScope(entries == null || entries.Length == 0))
                    if (GUILayout.Button("Save Prepared Data as ZIP (folders by type)"))
                        SaveZip();
                PatchUploadConfig config = PatchUploadConfig.LoadOrCreate();
                using (new EditorGUI.DisabledScope(entries == null || entries.Length == 0 || reviewedScope != Scope || database == null ||
                    string.IsNullOrWhiteSpace(database.patchDatabaseId) || string.IsNullOrWhiteSpace(database.dataPatchSettings.serviceUrl) ||
                    string.IsNullOrWhiteSpace(config.secretKey) || EditorApplication.isPlayingOrWillChangePlaymode))
                    if (GUILayout.Button("Upload and Publish Prepared Patch"))
                        Upload().Forget();
                if (string.IsNullOrWhiteSpace(config.secretKey))
                    EditorGUILayout.HelpBox("Configure the upload secret on the patch profile before uploading.", MessageType.Warning);
            }
            if (building)
            {
                Rect rect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
                EditorGUI.ProgressBar(rect, total == 0 ? 0 : (float)completed / total, $"Preparing {completed} / {total} records");
                if (GUILayout.Button("Cancel Preparation"))
                    buildCancellation?.Cancel();
            }
            if (entries != null && reviewedScope != Scope)
                EditorGUILayout.HelpBox("The patch target changed. Prepare a new snapshot before uploading.", MessageType.Warning);
            if (!string.IsNullOrEmpty(result))
                EditorGUILayout.HelpBox(result, MessageType.Info);
            using (new EditorGUI.DisabledScope(!DataPatchBuildLog.HasLog(database)))
                if (GUILayout.Button("Show Log"))
                    DataPatchBuildLog.Show(database);
            EditorGUILayout.HelpBox(partialPublishing ? "Selected records update the latest release. Omitted records remain active. Preview the prepared updates before uploading." : "Complete publication replaces the active snapshot. Records omitted from it return to bundled values.", MessageType.Info);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (string summary in typeSummaries)
                EditorGUILayout.LabelField(summary);
            EditorGUILayout.EndScrollView();
        }

        private void LoadDataTypes()
        {
            try
            {
                var log = new DataPatchBuildLog(database);
                var registry = DataPatchSnapshotBuilder.CreateRegistry(database, log);
                availableTypes = registry.Targets.Values.Select(asset => asset.GetType().Name).Distinct().OrderBy(type => type, StringComparer.Ordinal).ToArray();
                selectedTypes.RemoveWhere(type => !availableTypes.Contains(type));
                if (log.ErrorCount > 0)
                    log.Save("Loaded types for upload selection.");
            }
            catch (Exception ex)
            {
                availableTypes = Array.Empty<string>();
                result = ex.Message;
            }
        }

        private void DrawDataTypes()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select All Types"))
                    selectedTypes.UnionWith(availableTypes);
                if (GUILayout.Button("Select None"))
                    selectedTypes.Clear();
                if (GUILayout.Button("Refresh Types"))
                    LoadDataTypes();
            }
            typeScroll = EditorGUILayout.BeginScrollView(typeScroll, GUILayout.Height(150));
            foreach (string type in availableTypes)
            {
                bool selected = selectedTypes.Contains(type);
                bool value = EditorGUILayout.ToggleLeft(type, selected);
                if (value && !selected)
                    selectedTypes.Add(type);
                else if (!value && selected)
                    selectedTypes.Remove(type);
            }
            EditorGUILayout.EndScrollView();
        }

        private async UniTaskVoid BuildSnapshot()
        {
            building = true;
            entries = null;
            typeSummaries = Array.Empty<string>();
            completed = 0;
            total = 0;
            string scope = Scope;
            buildCancellation = new CancellationTokenSource();
            try
            {
                DataPatchEntry[] snapshot = await DataPatchSnapshotBuilder.Build(database, (done, count) =>
                {
                    completed = done;
                    total = count;
                    if (this != null)
                        Repaint();
                }, buildCancellation.Token, partialPublishing ? new HashSet<string>(selectedTypes) : null);
                if (this == null)
                    return;
                if (scope != Scope)
                    throw new InvalidOperationException("The patch target changed during preparation. Prepare a new snapshot.");
                entries = snapshot;
                reviewedScope = scope;
                typeSummaries = entries.GroupBy(entry => entry.dataType).Select(group => $"{group.Key}: {group.Count()} records").ToArray();
                result = $"Prepared {entries.Length} records. See Show Log for collected errors.";
            }
            catch (OperationCanceledException)
            {
                result = "Preparation cancelled. No partial snapshot will be uploaded.";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {
                buildCancellation.Dispose();
                buildCancellation = null;
                building = false;
                if (this != null)
                    Repaint();
            }
        }

        private void SaveZip()
        {
            string folder = EditorUtility.OpenFolderPanel("Choose folder for patch ZIP", "", "");
            if (string.IsNullOrEmpty(folder))
                return;
            try
            {
                string path = DataPatchZipExporter.SaveToFolder(folder, entries);
                result = $"Saved all {entries.Length} prepared records to {path}";
                EditorUtility.RevealInFinder(path);
            }
            catch (Exception ex)
            {
                result = "Could not save patch ZIP: " + ex.Message;
            }
        }

        private void OnDisable()
        {
            buildCancellation?.Cancel();
        }

        private async UniTaskVoid Upload()
        {
            uploading = true;
            try
            {
                if (entries == null || entries.Length == 0 || reviewedScope != Scope || string.IsNullOrWhiteSpace(database.patchDatabaseId))
                    throw new InvalidOperationException("Prepare a snapshot before uploading.");
                PatchUploadConfig config = PatchUploadConfig.LoadOrCreate();
                if (string.IsNullOrWhiteSpace(config.secretKey))
                    throw new InvalidOperationException("Upload secret is required.");
                if (!Uri.TryCreate(database.dataPatchSettings.serviceUrl, UriKind.Absolute, out Uri uri) || uri.Scheme != "https" && !uri.IsLoopback)
                    throw new InvalidOperationException("Use HTTPS, or loopback HTTP for local development.");
                List<string> chunks = BuildUploadChunks(out string manifestJson);
                string bodyHash = DataPatchHttp.Hash(manifestJson + "\n" + description + "\n" + database.dataPatchSettings.serviceUrl + "\n" + database.patchDatabaseId + "\n" + partialPublishing);
                if (config.pendingPayloadHash != bodyHash || config.pendingDatabaseId != database.patchDatabaseId || string.IsNullOrEmpty(config.pendingUploadId))
                {
                    config.pendingBaseReleaseId = partialPublishing ? await GetLatestReleaseId() : "";
                    config.pendingUploadId = Guid.NewGuid().ToString("N");
                }
                config.pendingPayloadHash = bodyHash;
                config.pendingDatabaseId = database.patchDatabaseId;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                string id = config.pendingUploadId;
                string json = JsonConvert.SerializeObject(new
                {
                id, databaseId = database.patchDatabaseId, schemaVersion = 2, description, manifestJson
                }

                );
                string serviceUrl = database.dataPatchSettings.serviceUrl;
                string secret = config.secretKey;
                int timeout = database.dataPatchSettings.requestTimeoutSeconds;
                EditorUtility.DisplayProgressBar("Upload Patch", "Creating upload manifest", 0);
                await Post(serviceUrl, secret, timeout, "/game-data-patches/chunked", json);
                string statusJson = await Post(serviceUrl, secret, timeout, "/game-data-patches/" + id + "/upload-status", "{}");
                var uploaded = new HashSet<int>(JObject.Parse(statusJson)["uploadedIndices"].Values<int>());
                for (int i = 0; i < chunks.Count; ++i)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Upload Patch", $"Chunk {i + 1} / {chunks.Count}", (float)i / Math.Max(1, chunks.Count)))
                        throw new OperationCanceledException("Upload paused. Retry will resume the same release.");
                    if (uploaded.Contains(i))
                        continue;
                    await Post(serviceUrl, secret, timeout, "/game-data-patches/" + id + "/chunks/" + i, JsonConvert.SerializeObject(new { payloadJson = chunks[i] }));
                }
                EditorUtility.DisplayProgressBar("Upload Patch", "Verifying and publishing all chunks", 1);
                string publishRoute = "/game-data-patches/" + id + (partialPublishing ? "/publish-partial" : "/publish");
                string publishBody = partialPublishing ? JsonConvert.SerializeObject(new { baseReleaseId = config.pendingBaseReleaseId }) : "{}";
                string published = await Post(serviceUrl, secret, timeout, publishRoute, publishBody);
                DataPatchRelease release = JsonConvert.DeserializeObject<DataPatchRelease>(published);
                DataPatchManifest publishedManifest = DataPatchHttp.ValidateManifest(release);
                if (release.databaseId != database.patchDatabaseId || string.IsNullOrEmpty(release.publishTime))
                    throw new InvalidOperationException("Published patch scope or publication does not match.");
                if (partialPublishing)
                {
                    DataPatchPartialPublication receipt = publishedManifest?.partial;
                    if (release.id != DataPatchHttp.Hash("partial:" + id).Substring(0, 32) || receipt == null ||
                        receipt.uploadId != id || receipt.baseReleaseId != config.pendingBaseReleaseId || receipt.uploadHash != DataPatchHttp.Hash(manifestJson))
                        throw new InvalidOperationException("Published partial patch does not match the prepared updates.");
                }
                else if (release.id != id || release.payloadHash != DataPatchHttp.Hash(manifestJson))
                    throw new InvalidOperationException("Published patch does not match the prepared manifest.");
                result = $"Published release {release.id}, version {release.version}, {entries.Length} records.";
                config.pendingUploadId = "";
                config.pendingBaseReleaseId = "";
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Patch Upload Successful", result, "OK");
            }
            catch (Exception ex)
            {
                result = ex.Message;
                if (partialPublishing && ex is PatchUploadException uploadError && uploadError.StatusCode == 409 &&
                    ex.Message.IndexOf("latest release changed", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    PatchUploadConfig config = PatchUploadConfig.LoadOrCreate();
                    config.pendingUploadId = "";
                    config.pendingBaseReleaseId = "";
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                    result += "\nRetry starts a new upload against the latest release.";
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                uploading = false;
                if (this != null)
                    Repaint();
            }
        }

        private List<string> BuildUploadChunks(out string manifestJson)
        {
            var payloads = new List<string>();
            var descriptors = new List<DataPatchChunk>();
            var records = new List<string>();
            int bytes = 2;
            string dataType = null;
            Action flush = () =>
            {
                if (records.Count == 0)
                    return;
                string payload = "[" + string.Join(",", records) + "]";
                descriptors.Add(new DataPatchChunk
                {
                    index = payloads.Count,
                    dataType = dataType,
                    hash = DataPatchHttp.Hash(payload),
                    bytes = Encoding.UTF8.GetByteCount(payload),
                    entryCount = records.Count,
                });
                payloads.Add(payload);
                records.Clear();
                bytes = 2;
            };
            foreach (DataPatchEntry entry in entries.OrderBy(value => value.dataType, StringComparer.Ordinal).ThenBy(value => value.dataId))
            {
                string record = JsonConvert.SerializeObject(entry);
                int recordBytes = Encoding.UTF8.GetByteCount(record);
                if (recordBytes + 2 > DataPatchHttp.MaxChunkBytes)
                    throw new InvalidOperationException($"{entry.dataType}/{entry.dataId} exceeds the 1 MiB per-record limit.");
                if (dataType != entry.dataType || bytes + recordBytes + 1 > 512 * 1024)
                    flush();
                dataType = entry.dataType;
                bytes += recordBytes + (records.Count > 0 ? 1 : 0);
                records.Add(record);
            }
            flush();
            manifestJson = JsonConvert.SerializeObject(new DataPatchManifest { chunks = descriptors.ToArray() });
            DataPatchHttp.ValidateManifest(new DataPatchRelease
            {
                id = "preview",
                schemaVersion = 2,
                manifestJson = manifestJson,
                payloadHash = DataPatchHttp.Hash(manifestJson),
            });
            return payloads;
        }

        private sealed class PatchUploadException : InvalidOperationException
        {
            public long StatusCode { get; }

            public PatchUploadException(long statusCode, string message) : base(message)
            {
                StatusCode = statusCode;
            }
        }

        private async UniTask<string> GetLatestReleaseId()
        {
            string url = database.dataPatchSettings.serviceUrl.TrimEnd('/') + "/game-data-patches/latest?databaseId=" + Uri.EscapeDataString(database.patchDatabaseId);
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = Math.Max(1, Math.Min(120, database.dataPatchSettings.requestTimeoutSeconds));
                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException)
                {
                    if (request.responseCode == 404)
                        return "";
                    throw new PatchUploadException(request.responseCode, GetUploadError(request, ""));
                }
                if (request.result != UnityWebRequest.Result.Success)
                    throw new PatchUploadException(request.responseCode, GetUploadError(request, ""));
                var release = JsonConvert.DeserializeObject<DataPatchRelease>(request.downloadHandler.text);
                DataPatchHttp.ValidateManifest(release);
                if (release.databaseId != database.patchDatabaseId || string.IsNullOrEmpty(release.publishTime))
                    throw new InvalidOperationException("Latest patch has an invalid scope or publication.");
                return release.id;
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
                    throw new PatchUploadException(request.responseCode, GetUploadError(request, secret));
                }

                if (request.result != UnityWebRequest.Result.Success)
                    throw new PatchUploadException(request.responseCode, GetUploadError(request, secret));
                return request.downloadHandler.text;
            }
        }

        private static string GetUploadError(UnityWebRequest request, string secret)
        {
            string detail = request.error;
            string response = request.downloadHandler?.text;
            if (!string.IsNullOrWhiteSpace(response))
            {
                try
                {
                    JObject body = JObject.Parse(response);
                    JToken message = body["message"] ?? body["error"];
                    if (message is JArray messages)
                        detail = string.Join("\n", messages.Values<string>());
                    else if (message?.Type == JTokenType.String)
                        detail = message.Value<string>();
                }
                catch (JsonException)
                {
                    // Non-JSON responses retain the HTTP error instead of displaying HTML.
                }
            }

            if (!string.IsNullOrEmpty(secret) && !string.IsNullOrEmpty(detail))
                detail = detail.Replace(secret, "[redacted]");

            return $"Patch upload failed (HTTP {request.responseCode}).\n{detail}\nRetry uses the same upload ID.";
        }
    }
}
