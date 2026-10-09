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
            Engine = new DataPatchEngine(database, refresh: false);
            try
            {
                await Engine.RefreshAsync();
                await Prepare(string.Empty);
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
            DataPatchEngine engine = Engine;
            DataPatchProfile profile = Profile;
            try
            {
                DataPatchRelease release = baseline ? null : await DataPatchHttp.Load(profile, id);
                if (Engine != engine)
                    throw new OperationCanceledException("Patch runtime was reset.");
                await engine.PrepareAsync(release);
                try
                {
                    // Save before activation so the synchronous commit performs no file IO or hashing.
                    await DataPatchHttp.CacheAsync(profile, release);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Game-data patch prepared, but its cache could not be saved: " + ex.Message);
                }
                if (Engine != engine)
                    throw new OperationCanceledException("Patch runtime was reset.");
                LastError = "";
            }
            catch (Exception ex)
            {
                if (Engine == engine)
                    LastError = ex.Message;
                throw;
            }
            finally
            {
                if (Engine == engine)
                    IsLoading = false;
            }
        }

        public static void Commit()
        {
            DataPatchRelease release = Engine.PreparedRelease;
            Engine.Commit();
            ActiveHash = release?.payloadHash ?? "";
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
