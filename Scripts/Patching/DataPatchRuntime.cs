using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class DataPatchRuntime
    {
        private static UniTaskCompletionSource<Exception> initializationCompletion;
        private static bool initializationStarted;
        private static int runtimeVersion;

        public static DataPatchEngine Engine { get; private set; }

        public static bool IsLoading { get; private set; }

        public static bool IsReady { get; private set; }

        public static string LastError { get; private set; } = "";
        public static string ActiveReleaseId => Engine?.ActiveReleaseId ?? "";
        public static string ActiveHash { get; private set; } = "";
        public static double LastDownloadMilliseconds { get; private set; }
        public static DataPatchProfile Profile { get; private set; }

        public static void StartInitialization(DataPatchProfile profile, GameDatabase loadedDatabase)
        {
            UniTask initialization = Initialize(profile, loadedDatabase);
            var completion = new UniTaskCompletionSource<Exception>();
            initializationCompletion = completion;
            initializationStarted = true;
            CompleteInitialization(initialization, completion, runtimeVersion).Forget();
        }

        private static async UniTaskVoid CompleteInitialization(UniTask initialization, UniTaskCompletionSource<Exception> completion, int version)
        {
            Exception failure = null;
            try
            {
                await initialization;
            }
            catch (Exception error)
            {
                failure = error;
                if (version == runtimeVersion && LastError != error.Message)
                {
                    LastError = error.Message;
                    Debug.LogError("Game-data patch startup failed: " + error.Message);
                }
            }
            // CompletionSource supports concurrent waiters; failures are data until a gate awaits them.
            completion.TrySetResult(failure);
        }

        public static async UniTask WaitForInitialization(bool requireReady = true)
        {
            int version = runtimeVersion;
            Exception failure = initializationStarted ? await initializationCompletion.Task : null;
            if (version != runtimeVersion)
                throw new OperationCanceledException("Patch runtime was reset.");
            if (failure != null && (requireReady || Engine == null))
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
            // A server-selected exact release can recover a failed latest lookup on the client.
            if (requireReady && !IsReady)
                throw new InvalidOperationException(string.IsNullOrEmpty(LastError) ? "Patch initialization is not ready." : LastError);
        }

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
            DataPatchEngine initializingEngine = Engine;
            var startup = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                // Register scanned references before menus use GameInstance ID lookups; no JSON snapshot.
                Engine.RefreshRegistry();
                await Prepare(string.Empty, refreshRegistry: false);
                if (Engine != initializingEngine)
                    throw new OperationCanceledException("Patch runtime was reset.");
                Commit();
                IsReady = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[Data Patch] Startup finished in {startup.Elapsed.TotalMilliseconds:F1} ms: download {LastDownloadMilliseconds:F1} ms, preparation {Engine.LastPreparationMilliseconds:F1} ms, activation {Engine.LastCommitMilliseconds:F1} ms.");
#endif
            }
            catch (Exception ex)
            {
                if (Engine == initializingEngine)
                {
                    LastError = ex.Message;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError($"[Data Patch] Startup failed after {startup.Elapsed.TotalMilliseconds:F1} ms; download {LastDownloadMilliseconds:F1} ms. {ex.Message}");
#endif
                }
                throw;
            }
        }

        public static async UniTask Prepare(string id, bool baseline = false, bool refreshRegistry = true, string expectedHash = null)
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
                var download = System.Diagnostics.Stopwatch.StartNew();
                DataPatchRelease release;
                try
                {
                    release = baseline ? null : await DataPatchHttp.Load(profile, id);
                }
                finally
                {
                    if (Engine == engine)
                        LastDownloadMilliseconds = download.Elapsed.TotalMilliseconds;
                }
                if (Engine != engine)
                    throw new OperationCanceledException("Patch runtime was reset.");
                if (expectedHash != null && (release?.payloadHash ?? "") != expectedHash)
                    throw new InvalidOperationException("Server/broadcast patch hash does not match the downloaded release.");
                await engine.PrepareAsync(release, refreshRegistry: refreshRegistry);
                try
                {
                    // Save before activation so the synchronous commit performs no file IO or hashing.
                    await DataPatchHttp.CachePreparedAsync(profile, release);
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
            IsReady = true;
            LastError = "";
            Debug.Log($"Game-data patch activated: {ActiveReleaseId}");
        }

        public static void Reset()
        {
            ++runtimeVersion;
            initializationStarted = false;
            initializationCompletion = null;
            Engine?.Dispose();
            Engine = null;
            Profile = null;
            IsReady = false;
            IsLoading = false;
            LastError = "";
            ActiveHash = "";
            LastDownloadMilliseconds = 0;
        }
    }
}
