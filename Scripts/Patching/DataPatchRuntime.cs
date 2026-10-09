using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class DataPatchRuntime
    {
        public static DataPatchEngine Engine { get; private set; }

        public static bool IsLoading { get; private set; }

        public static bool IsReady { get; private set; }

        public static string LastError { get; private set; } = "";
        public static string ActiveReleaseId => Engine?.ActiveReleaseId ?? "";
        public static string ActiveHash { get; private set; } = "";
        public static DataPatchProfile Profile { get; private set; }

        public static async UniTask Initialize(DataPatchProfile database, GameDatabase loadedDatabase)
        {
            Reset();
            if (database == null || !database.dataPatchSettings.enabled)
            {
                IsReady = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(database.patchDatabaseId))
                throw new InvalidOperationException("Assign the patch profile database ID before enabling patches.");
            if (database.database != loadedDatabase)
                throw new InvalidOperationException("Patch profile references a different game database.");
            Profile = database;
            Engine = new DataPatchEngine(database);
            try
            {
#if !UNITY_SERVER
                // Clients select the exact server release during the connection handshake.
                if (!database.dataPatchSettings.loadAtStartup)
                {
                    IsReady = true;
                    return;
                }

#endif
                await Prepare(database.dataPatchSettings.startupReleaseId);
                Commit();
                IsReady = true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                throw;
            }
        }

        public static async UniTask Prepare(string id, bool baseline = false)
        {
            if (IsLoading)
                throw new InvalidOperationException("A patch load is already in progress.");
            if (Engine == null)
                throw new InvalidOperationException("Enable data patching on the GameInstance patch profile first.");
            IsLoading = true;
            try
            {
                DataPatchRelease release = baseline ? null : await DataPatchHttp.Load(Profile, id);
                Engine.Prepare(release);
                LastError = "";
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                throw;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public static void Commit()
        {
            DataPatchRelease release = Engine.PreparedRelease;
            Engine.Commit();
            ActiveHash = release?.payloadHash ?? "";
            try
            {
                DataPatchHttp.Cache(Profile, release);
            }
            catch (Exception)
            {
                Debug.LogWarning("Game-data patch applied, but its cache could not be saved.");
            }

            Debug.Log($"Game-data patch activated: {ActiveReleaseId}");
        }

        public static void Reset()
        {
            Engine?.Dispose();
            Engine = null;
            Profile = null;
            IsReady = false;
            IsLoading = false;
            LastError = "";
            ActiveHash = "";
        }
    }
}
