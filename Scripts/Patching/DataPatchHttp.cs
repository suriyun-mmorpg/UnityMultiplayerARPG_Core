using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace MultiplayerARPG
{
    public static class DataPatchHttp
    {
        public static string Hash(string text)
        {
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
        }

        public static void ValidateHash(DataPatchRelease release)
        {
            if (release == null || string.IsNullOrEmpty(release.id) || string.IsNullOrEmpty(release.payloadJson) || Encoding.UTF8.GetByteCount(release.payloadJson) > 3 * 1024 * 1024 || !string.Equals(Hash(release.payloadJson), release.payloadHash, StringComparison.Ordinal))
                throw new InvalidOperationException("Patch integrity check failed.");
        }

        public static async UniTask<DataPatchRelease> Load(DataPatchProfile database, string releaseId = "")
        {
            DataPatchSettings settings = database.dataPatchSettings;
            if (!Uri.TryCreate(settings.serviceUrl, UriKind.Absolute, out Uri uri) || (uri.Scheme != "https" && !uri.IsLoopback))
                throw new InvalidOperationException("Patch service requires HTTPS (HTTP allowed only on loopback).");
            string route = string.IsNullOrEmpty(releaseId) ? "latest?databaseId=" + Uri.EscapeDataString(database.patchDatabaseId) + "&environment=" + Uri.EscapeDataString(settings.environment) : Uri.EscapeDataString(releaseId);
            string url = settings.serviceUrl.TrimEnd('/') + "/game-data-patches/" + route;
            DataPatchRelease release;
            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = Math.Max(1, Math.Min(120, settings.requestTimeoutSeconds));
                    try
                    {
                        await request.SendWebRequest();
                    }
                    catch (UnityWebRequestException)when (request.responseCode == 404 && string.IsNullOrEmpty(releaseId))
                    {
                        return null;
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                        throw new IOException("Patch download failed.");
                    release = JsonConvert.DeserializeObject<DataPatchRelease>(request.downloadHandler.text);
                }
            }
            catch (Exception ex)when (ex is IOException || ex is UnityWebRequestException)
            {
                // Never substitute another release or guess the latest during an outage.
                if (string.IsNullOrEmpty(releaseId) || !File.Exists(CachePath(database, releaseId)))
                    throw;
                release = JsonConvert.DeserializeObject<DataPatchRelease>(File.ReadAllText(CachePath(database, releaseId)));
            }

            ValidateHash(release);
            if (!string.IsNullOrEmpty(releaseId) && release.id != releaseId)
                throw new InvalidOperationException("Unexpected patch release ID.");
            return release;
        }

        private static string CachePath(DataPatchProfile database, string id) => Path.Combine(Application.persistentDataPath, "GameDataPatches", Hash(database.patchDatabaseId + "/" + database.dataPatchSettings.environment + "/" + id) + ".json");

        public static void Cache(DataPatchProfile database, DataPatchRelease release)
        {
            if (release == null)
                return;
            string path = CachePath(database, release.id);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonConvert.SerializeObject(release));
            if (File.Exists(path))
                File.Replace(temp, path, null);
            else
                File.Move(temp, path);
        }
    }
}
