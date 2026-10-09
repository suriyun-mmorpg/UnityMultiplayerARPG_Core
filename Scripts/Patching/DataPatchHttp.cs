using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace MultiplayerARPG
{
    public static class DataPatchHttp
    {
        public const int MaxChunkBytes = 1024 * 1024;
        public const int MaxReleaseBytes = 128 * 1024 * 1024;
        public const int MaxManifestBytes = 256 * 1024;
        private const int MaxResponseBytes = 8 * 1024 * 1024;
        public static event Action<float, string> DownloadProgress;

        public static string Hash(string text)
        {
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
        }

        public static DataPatchManifest ValidateManifest(DataPatchRelease release)
        {
            if (release == null || string.IsNullOrEmpty(release.id))
                throw new InvalidOperationException("Invalid patch release.");
            if (release.schemaVersion == 1)
            {
                if (string.IsNullOrEmpty(release.payloadJson) || Encoding.UTF8.GetByteCount(release.payloadJson) > 3 * 1024 * 1024 || Hash(release.payloadJson) != release.payloadHash)
                    throw new InvalidOperationException("Patch integrity check failed.");
                return null;
            }
            if (release.schemaVersion != 2 || string.IsNullOrEmpty(release.manifestJson) ||
                Encoding.UTF8.GetByteCount(release.manifestJson) > MaxManifestBytes || Hash(release.manifestJson) != release.payloadHash)
                throw new InvalidOperationException("Patch manifest integrity check failed.");
            var manifest = JsonConvert.DeserializeObject<DataPatchManifest>(release.manifestJson);
            if (manifest == null || manifest.schemaVersion != 2 || manifest.chunks == null || manifest.chunks.Length > 1024)
                throw new InvalidOperationException("Invalid patch manifest.");
            long bytes = 0;
            int entries = 0;
            for (int i = 0; i < manifest.chunks.Length; ++i)
            {
                DataPatchChunk chunk = manifest.chunks[i];
                if (chunk == null || chunk.index != i || string.IsNullOrEmpty(chunk.dataType) ||
                    !Regex.IsMatch(chunk.dataType, "^[A-Za-z_][A-Za-z0-9_]{0,127}$") ||
                    string.IsNullOrEmpty(chunk.hash) || !Regex.IsMatch(chunk.hash, "^[a-f0-9]{64}$") ||
                    chunk.bytes < 2 || chunk.bytes > MaxChunkBytes || chunk.entryCount < 1 || chunk.entryCount > 20000)
                    throw new InvalidOperationException("Invalid patch chunk descriptor.");
                bytes += chunk.bytes;
                entries += chunk.entryCount;
            }
            if (bytes > MaxReleaseBytes || entries > 20000)
                throw new InvalidOperationException("Patch exceeds 128 MiB or 20000 records.");
            return manifest;
        }

        public static void ValidateHash(DataPatchRelease release)
        {
            DataPatchManifest manifest = ValidateManifest(release);
            if (manifest == null)
                return;
            if (release.chunkPaths == null || release.chunkPaths.Length != manifest.chunks.Length)
                throw new InvalidOperationException("Patch chunks have not finished downloading.");
            for (int i = 0; i < manifest.chunks.Length; ++i)
                ReadChunkFile(release.chunkPaths[i], manifest.chunks[i]);
        }

        private static string ReadChunkFile(string path, DataPatchChunk chunk)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path) || new FileInfo(path).Length != chunk.bytes)
                throw new IOException("Missing or incomplete patch chunk " + chunk.index);
            string payload = File.ReadAllText(path, Encoding.UTF8);
            ValidateChunkText(payload, chunk);
            return payload;
        }

        private static void ValidateChunkText(string payload, DataPatchChunk chunk)
        {
            if (payload == null || Encoding.UTF8.GetByteCount(payload) != chunk.bytes || Hash(payload) != chunk.hash)
                throw new IOException("Patch chunk integrity check failed: " + chunk.index);
        }

        public static IEnumerable<DataPatchEntry> ReadEntries(DataPatchRelease release)
        {
            DataPatchManifest manifest = ValidateManifest(release);
            if (manifest == null)
            {
                var entries = JsonConvert.DeserializeObject<DataPatchEntry[]>(release.payloadJson);
                if (entries == null || entries.Length > 20000)
                    throw new InvalidOperationException("Invalid patch entry count.");
                foreach (DataPatchEntry entry in entries)
                    yield return entry;
                yield break;
            }
            if (release.chunkPaths == null || release.chunkPaths.Length != manifest.chunks.Length)
                throw new InvalidOperationException("Patch chunks are incomplete.");
            for (int i = 0; i < manifest.chunks.Length; ++i)
            {
                DataPatchChunk chunk = manifest.chunks[i];
                var entries = JsonConvert.DeserializeObject<DataPatchEntry[]>(ReadChunkFile(release.chunkPaths[i], chunk));
                if (entries == null || entries.Length != chunk.entryCount)
                    throw new InvalidOperationException("Patch chunk record count mismatch.");
                foreach (DataPatchEntry entry in entries)
                {
                    if (entry == null || entry.dataType != chunk.dataType)
                        throw new InvalidOperationException("Patch chunk data type mismatch.");
                    yield return entry;
                }
            }
        }

        public static async UniTask<DataPatchRelease> Load(DataPatchProfile database, string releaseId = "")
        {
            DataPatchSettings settings = database.dataPatchSettings;
            if (!Uri.TryCreate(settings.serviceUrl, UriKind.Absolute, out Uri uri) || (uri.Scheme != "https" && !uri.IsLoopback))
                throw new InvalidOperationException("Patch service requires HTTPS (HTTP allowed only on loopback).");
            string route = string.IsNullOrEmpty(releaseId) ? "latest?databaseId=" + Uri.EscapeDataString(database.patchDatabaseId) + "&environment=" + Uri.EscapeDataString(settings.environment) : Uri.EscapeDataString(releaseId);
            string baseUrl = settings.serviceUrl.TrimEnd('/') + "/game-data-patches/";
            DataPatchRelease release;
#if UNITY_EDITOR
            Debug.Log($"[Data Patch] Download started. Database: {database.patchDatabaseId}, environment: {settings.environment}, release: {(string.IsNullOrEmpty(releaseId) ? "latest" : releaseId)}.");
#endif
            ReportProgress(0, "Loading patch manifest");
            try
            {
                string json = await Get(baseUrl + route, settings.requestTimeoutSeconds, string.IsNullOrEmpty(releaseId));
                if (json == null)
                {
#if UNITY_EDITOR
                    Debug.Log($"[Data Patch] Download check finished. No published patch for database {database.patchDatabaseId}, environment {settings.environment}.");
#endif
                    ReportProgress(1, "No published patch");
                    return null;
                }
                release = await RunBackground(() => JsonConvert.DeserializeObject<DataPatchRelease>(json));
            }
            catch (Exception ex) when (ex is IOException || ex is UnityWebRequestException)
            {
                // Latest is never guessed during an outage. Only an exact cached ID is allowed.
                string path = CachePath(database, releaseId);
                if (string.IsNullOrEmpty(releaseId) || !File.Exists(path) || new FileInfo(path).Length > MaxResponseBytes)
                    throw;
                release = await RunBackground(() => JsonConvert.DeserializeObject<DataPatchRelease>(File.ReadAllText(path)));
            }
            DataPatchManifest manifest = ValidateManifest(release);
            if ((!string.IsNullOrEmpty(releaseId) && release.id != releaseId) || release.databaseId != database.patchDatabaseId ||
                release.environment != settings.environment || string.IsNullOrEmpty(release.publishTime))
                throw new InvalidOperationException("Patch ID, scope or publication mismatch.");
            if (manifest != null)
            {
                release.chunkPaths = new string[manifest.chunks.Length];
                for (int i = 0; i < manifest.chunks.Length; ++i)
                {
                    DataPatchChunk chunk = manifest.chunks[i];
                    string path = ChunkPath(chunk.hash);
                    bool cached = false;
                    try
                    {
                        await RunBackground(() => ReadChunkFile(path, chunk));
                        cached = true;
                    }
                    catch (IOException)
                    {
                        // A truncated or corrupt cache entry is replaced only by verified data.
                    }
                    if (!cached)
                    {
                        string url = baseUrl + Uri.EscapeDataString(release.id) + "/chunks/" + i;
                        for (int attempt = 0; ; ++attempt)
                        {
                            try
                            {
                                string response = await Get(url, settings.requestTimeoutSeconds, false);
                                await RunBackground(() =>
                                {
                                    var body = JsonConvert.DeserializeObject<ChunkResponse>(response);
                                    ValidateChunkText(body?.payloadJson, chunk);
                                    AtomicWrite(path, body.payloadJson);
                                    return true;
                                });
                                break;
                            }
                            catch (Exception ex) when (attempt < 2 && (ex is IOException || ex is UnityWebRequestException))
                            {
                                await UniTask.Delay(250 * (attempt + 1));
                            }
                        }
                    }
                    release.chunkPaths[i] = path;
                    ReportProgress((float)(i + 1) / Math.Max(1, manifest.chunks.Length), "Patch chunk " + (i + 1) + "/" + manifest.chunks.Length);
                }
            }
            await RunBackground(() =>
            {
                ValidateHash(release);
                return true;
            });
#if UNITY_EDITOR
            Debug.Log($"[Data Patch] Download finished and integrity verified. Release: {release.id}, version: {release.version}, database: {release.databaseId}, environment: {release.environment}.");
#endif
            ReportProgress(1, "Patch download verified");
            return release;
        }

        private static void ReportProgress(float value, string message)
        {
            try
            {
                DownloadProgress?.Invoke(value, message);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static async UniTask<string> Get(string url, int timeout, bool allowMissing)
        {
            using (var request = new UnityWebRequest(url, "GET"))
            {
                var handler = new BoundedDownload();
                request.downloadHandler = handler;
                request.timeout = Math.Max(1, Math.Min(120, timeout));
                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException) when (allowMissing && request.responseCode == 404)
                {
                    return null;
                }
                if (request.result != UnityWebRequest.Result.Success)
                    throw new IOException("Patch download failed (HTTP " + request.responseCode + ").");
                return handler.Text;
            }
        }

        private sealed class ChunkResponse
        {
            public string payloadJson;
        }

        private sealed class BoundedDownload : DownloadHandlerScript
        {
            private readonly MemoryStream buffer = new MemoryStream();
            public string Text => Encoding.UTF8.GetString(buffer.ToArray());

            public BoundedDownload() : base(new byte[64 * 1024])
            {
            }

            protected override bool ReceiveData(byte[] data, int length)
            {
                if (data == null || length < 0 || buffer.Length + length > MaxResponseBytes)
                    return false;
                buffer.Write(data, 0, length);
                return true;
            }
        }

        private static string CachePath(DataPatchProfile database, string id)
        {
            return Path.Combine(Application.persistentDataPath, "GameDataPatches", Hash(database.patchDatabaseId + "/" + database.dataPatchSettings.environment + "/" + id) + ".json");
        }

        private static string ChunkPath(string hash)
        {
            return Path.Combine(Application.persistentDataPath, "GameDataPatches", "chunks", hash + ".json");
        }

        private static void AtomicWrite(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temp, text, new UTF8Encoding(false));
                if (File.Exists(path))
                    File.Replace(temp, path, null);
                else
                    File.Move(temp, path);
            }
            finally
            {
                if (File.Exists(temp))
                    File.Delete(temp);
            }
        }

        internal static async UniTask<T> RunBackground<T>(Func<T> work, CancellationToken cancellationToken = default)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            cancellationToken.ThrowIfCancellationRequested();
            return work();
#else
            T result = default;
            System.Runtime.ExceptionServices.ExceptionDispatchInfo error = null;
            try
            {
                result = await UniTask.RunOnThreadPool(work, configureAwait: false, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                error = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex);
            }
            // Return explicitly even on failure/cancellation before callers clean up Unity objects.
            await UniTask.SwitchToMainThread();
            if (error != null)
                error.Throw();
            cancellationToken.ThrowIfCancellationRequested();
            return result;
#endif
        }

        public static async UniTask CacheAsync(DataPatchProfile database, DataPatchRelease release)
        {
            if (release == null)
                return;
            // Resolve Unity configuration paths on the main thread before dispatching file work.
            string path = CachePath(database, release.id);
            await RunBackground(() =>
            {
                ValidateHash(release);
                AtomicWrite(path, JsonConvert.SerializeObject(release));
                return true;
            });
        }

        public static void Cache(DataPatchProfile database, DataPatchRelease release)
        {
            if (release == null)
                return;
            ValidateHash(release);
            AtomicWrite(CachePath(database, release.id), JsonConvert.SerializeObject(release));
        }
    }
}
